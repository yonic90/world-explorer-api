# World Explorer API

A full-stack web application that lets you look up information about any country — including capital, population, region, currency, flag, and current weather — by combining data from two free public APIs.

## Stack

- **Backend**: ASP.NET Core 10.0 Web API (minimal APIs pattern)
- **Frontend**: React + Vite
- **External APIs**: RestCountries, OpenWeatherMap

## Project Structure

```
world-explorer-api/
├── Program.cs                  # App entry point — services, middleware, endpoints
├── Models/
│   ├── CountryInfo.cs          # name, capital, flag, population, region, currency, language, lat/lng
│   └── WeatherInfo.cs          # description, temperature, feelsLike, humidity, icon
├── Services/
│   ├── CountryService.cs       # Calls RestCountries API
│   └── WeatherService.cs       # Calls OpenWeatherMap API
├── frontend/                   # React + Vite app
│   └── src/
│       ├── App.jsx
│       └── main.jsx
├── appsettings.json
├── docker-compose.yml
└── Dockerfile
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for the frontend)
- An [OpenWeatherMap API key](https://openweathermap.org/api) (free tier works)

### Configuration

Add your OpenWeatherMap API key to `appsettings.json`:

```json
"OpenWeatherMap": {
  "ApiKey": "your-key-here"
}
```

Weather data will be `null` silently if the key is missing or invalid.

### Running the Backend

```bash
dotnet build
dotnet run                          # HTTP on port 5252
dotnet run --launch-profile https   # HTTPS on port 7037
```

### Running the Frontend

```bash
cd frontend
npm install
npm run dev   # Starts on http://localhost:5173
```

## API

### `GET /api/country/{name}`

Returns country information and current weather.

**Example:** `GET /api/country/france`

```json
{
  "country": {
    "name": "France",
    "capital": "Paris",
    "flag": "🇫🇷",
    "population": 67391582,
    "region": "Europe",
    "currency": "Euro",
    "language": "French",
    "latitude": 46.0,
    "longitude": 2.0
  },
  "weather": {
    "description": "clear sky",
    "temperature": 14.2,
    "feelsLike": 13.1,
    "humidity": 60,
    "icon": "01d"
  }
}
```

`weather` is `null` if the OpenWeatherMap API key is missing or the call fails.

**404** is returned if the country name is not recognized.

### `GET /openapi/v1.json` *(development only)*

OpenAPI spec — view in any Swagger-compatible tool.

## Running with Docker

```bash
docker-compose up --build
```

## Manual Testing

Use the included `world-explorer-api.http` file with the VS Code REST Client extension or JetBrains HTTP Client to fire requests directly from the editor.

## External APIs

| API | Docs | Auth |
|-----|------|------|
| RestCountries | https://restcountries.com | None required |
| OpenWeatherMap | https://openweathermap.org/api | Free API key |
