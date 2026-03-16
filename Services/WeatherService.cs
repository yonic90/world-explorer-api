using System.Text.Json;
using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    // WeatherService fetches current weather data from OpenWeatherMap.
    // API docs: https://openweathermap.org/current
    // Requires a free API key — store it in appsettings.json under "OpenWeatherMap:ApiKey".
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // The API key is read once at construction time from IConfiguration,
        // which reads from appsettings.json (and appsettings.Development.json, env vars, etc.)
        // The "OpenWeatherMap:ApiKey" key uses ':' to navigate nested JSON sections:
        //   { "OpenWeatherMap": { "ApiKey": "abc123" } }
        private readonly string _apiKey;

        // IConfiguration is automatically available in the DI container — no extra
        // registration needed. It provides access to all app configuration sources.
        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? "";
        }

        public async Task<WeatherInfo?> GetWeatherAsync(double latitude, double longitude)
        {
            // Fail fast if the API key wasn't configured.
            // Returning null instead of throwing lets the endpoint still return
            // country data — just without the weather portion.
            if (string.IsNullOrEmpty(_apiKey))
                return null;

            var client = _httpClientFactory.CreateClient();

            // Build the request URL with query parameters:
            //   lat / lon  — coordinates from the country data
            //   appid      — the API key
            //   units=metric — returns temperature in Celsius (use "imperial" for Fahrenheit)
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            // The OpenWeatherMap response looks like:
            // {
            //   "weather": [{ "description": "clear sky", "icon": "01d" }],
            //   "main":    { "temp": 22.5, "feels_like": 21.0, "humidity": 60 }
            // }
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // "weather" is an array — take the first element (primary condition)
            var weather = root.GetProperty("weather")[0];

            // "main" contains the numeric measurements
            var main = root.GetProperty("main");

            return new WeatherInfo
            {
                Description = weather.GetProperty("description").GetString() ?? "",
                Temperature = main.GetProperty("temp").GetDouble(),
                FeelsLike = main.GetProperty("feels_like").GetDouble(),
                Humidity = main.GetProperty("humidity").GetInt32(),
                // Icon is a code like "01d" (clear sky, daytime).
                // The full image URL is: https://openweathermap.org/img/wn/{icon}@2x.png
                Icon = weather.GetProperty("icon").GetString() ?? ""
            };
        }
    }
}
