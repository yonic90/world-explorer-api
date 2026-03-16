# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                          # Build the project
dotnet run                            # Run on HTTP (port 5252)
dotnet run --launch-profile https     # Run on HTTPS (port 7037)
dotnet test                           # Run tests (when test project exists)
```

`world-explorer-api.http` can be used to manually test endpoints.

## Architecture

ASP.NET Core 10.0 Web API using the **minimal APIs** pattern — endpoints are registered directly in `Program.cs` via `app.Map*`, no controllers.

- `Models/` — `CountryInfo` (name, capital, flag, population, region, currency, language, lat/lng) and `WeatherInfo` (description, temperature, feelsLike, humidity, icon)
- `Services/` — `ICountryService`/`CountryService` calls RestCountries API; `IWeatherService`/`WeatherService` calls OpenWeatherMap API (requires key in config)

**Request flow for `GET /api/country/{name}`:**
1. `CountryService.GetCountryAsync` fetches from RestCountries, parses JSON manually via `JsonDocument`, returns `CountryInfo` with lat/lng
2. `WeatherService.GetWeatherAsync` uses the lat/lng to fetch from OpenWeatherMap, returns `WeatherInfo`
3. Endpoint returns `{ Country, Weather }` — `Weather` is `null` if API key is missing or call fails

## Configuration

OpenWeatherMap API key goes in `appsettings.json`:
```json
"OpenWeatherMap": { "ApiKey": "your-key-here" }
```
`WeatherService` returns `null` silently when the key is absent.

## External APIs

- **RestCountries**: `https://restcountries.com/v3.1/name/{countryName}` — no key required
- **OpenWeatherMap**: `https://api.openweathermap.org/data/2.5/weather` — key required

## Conventions

- Services registered as `Scoped` via DI in `Program.cs`
- `IHttpClientFactory` used for all HTTP calls (injected into services)
- All service methods are async; JSON parsed with `System.Text.Json.JsonDocument`
