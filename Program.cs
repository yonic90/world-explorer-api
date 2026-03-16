// ============================================================
// Program.cs — Application entry point
//
// ASP.NET Core uses a "minimal hosting model" (introduced in .NET 6).
// Everything — service registration, middleware, and endpoints — lives
// in this single file instead of Startup.cs + Program.cs like older versions.
// ============================================================

using WorldExplorerApi.Services;

// ----------------------------------------------------------
// STEP 1: Create the application builder.
// WebApplication.CreateBuilder sets up configuration (appsettings.json,
// environment variables, command-line args) and the DI container.
// ----------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------
// STEP 2: Register services into the DI (Dependency Injection) container.
//
// Services registered here can be injected into endpoints and other
// services via constructor parameters or endpoint parameters.
// ----------------------------------------------------------

// Adds OpenAPI (Swagger) support so you can browse/test the API at
// /openapi/v1.json in development. Only active in dev — see app.MapOpenApi() below.
builder.Services.AddOpenApi();

// Registers IHttpClientFactory, which is the recommended way to create
// HttpClient instances. It manages connection pooling and avoids socket
// exhaustion that can occur when creating HttpClient directly.
builder.Services.AddHttpClient();

// Register our custom services using the interface → implementation pattern.
// "Scoped" means one instance is created per HTTP request and shared
// within that request. This is the most common lifetime for services
// that touch external APIs or databases.
//
// Using interfaces (ICountryService, IWeatherService) instead of concrete
// classes makes it easy to swap implementations later (e.g. for testing).
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Configure CORS (Cross-Origin Resource Sharing).
// Browsers block JavaScript from calling APIs on a different origin
// (different domain or port) unless the server explicitly allows it.
// Our React dev server runs on http://localhost:5173, so we allow that origin.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()   // Allow Content-Type, Authorization, etc.
              .AllowAnyMethod()); // Allow GET, POST, PUT, DELETE, etc.
});

// ----------------------------------------------------------
// STEP 3: Build the WebApplication from the configured builder.
// After this point, no more services can be registered.
// ----------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------
// STEP 4: Configure the middleware pipeline.
//
// Middleware are components that handle requests in sequence.
// Order matters — each middleware calls the next one in the chain.
// ----------------------------------------------------------

// Only expose the OpenAPI endpoint in development.
// In production you'd typically serve docs separately or not at all.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Available at: GET /openapi/v1.json
}

// Redirect HTTP requests to HTTPS automatically.
app.UseHttpsRedirection();

// Apply the CORS policy registered above.
// Must come before endpoint routing so the OPTIONS preflight requests
// are handled before reaching the actual endpoint handlers.
app.UseCors();

// ----------------------------------------------------------
// STEP 5: Map endpoints (minimal API style).
//
// Instead of controllers and [HttpGet] attributes, minimal APIs
// use app.MapGet / MapPost / MapPut / MapDelete directly.
// Parameters are automatically resolved from the route, query string,
// or DI container depending on their type.
// ----------------------------------------------------------

// GET /api/country/{name}
// {name} is a route parameter — ASP.NET Core binds it to the 'name' argument.
// ICountryService and IWeatherService are injected from the DI container.
app.MapGet("/api/country/{name}", async (string name, ICountryService countryService, IWeatherService weatherService) =>
{
    // Ask the country service to fetch data from the RestCountries API.
    // Returns null if the country was not found or the request failed.
    var country = await countryService.GetCountryAsync(name);

    if (country is null)
        return Results.NotFound(); // 404 response

    // Use the country's coordinates to fetch current weather.
    // Returns null if the OpenWeatherMap API key is missing or the call fails.
    var weather = await weatherService.GetWeatherAsync(country.Latitude, country.Longitude);

    // Results.Ok() serialises the anonymous object to JSON and returns HTTP 200.
    // The response shape: { "country": { ... }, "weather": { ... } | null }
    return Results.Ok(new { Country = country, Weather = weather });
})
.WithName("GetCountry"); // Gives the endpoint a name (useful for link generation)

// Placeholder endpoint from the default Vite template — kept for reference.
// Demonstrates how to return data without any external dependencies.
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ----------------------------------------------------------
// STEP 6: Start the web server and begin listening for requests.
// ----------------------------------------------------------
app.Run();

// ----------------------------------------------------------
// WeatherForecast — a record used by the placeholder endpoint above.
//
// Records are immutable value-type-like classes introduced in C# 9.
// This one uses a positional constructor (parameters defined in the header).
// The TemperatureF property is computed from TemperatureC.
// ----------------------------------------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
