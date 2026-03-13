# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                          # Build the project
dotnet run                            # Run on HTTP (port 5252)
dotnet run --launch-profile https     # Run on HTTPS (port 7037)
dotnet test                           # Run tests (when test project exists)
```

The `.http` file at `world-explorer-api.http` can be used to manually test endpoints.

## Architecture

ASP.NET Core 10.0 Web API using the **minimal APIs** pattern (no controllers — endpoints are registered directly in `Program.cs` via `app.Map*` methods).

**Key structural intent:**
- `Models/` — Data transfer objects (`CountryInfo`, `WeatherInfo`). These are defined but not yet wired to endpoints.
- `Services/` — Empty; intended for business logic / external API integrations.
- `Controllers/` — Empty; not used (minimal API style).

**Middleware pipeline (Program.cs):**
1. OpenAPI (dev only)
2. HTTPS redirection
3. Endpoint routing

The project is early-stage. The only active endpoint is a placeholder `GET /weatherforecast`. `CountryInfo` and `WeatherInfo` models suggest the app is intended to serve real country and weather data, likely by integrating external APIs inside `Services/`.

## External APIs

- **RestCountries**: `https://restcountries.com/v3.1/name/{countryName}` — no API key required
- **OpenWeatherMap**: `https://api.openweathermap.org/data/2.5/weather` — requires API key (store in appsettings.json)

## Intended Endpoints

- `GET /api/country/{name}` — returns CountryInfo + WeatherInfo combined

## Conventions

- Services registered via Dependency Injection in Program.cs
- Use IHttpClientFactory for HTTP calls
- Interfaces for all services (ICountryService, IWeatherService)
- Async/await throughout
