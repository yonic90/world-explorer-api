using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    // Interface for weather data retrieval.
    // Takes coordinates (lat/lng) rather than a city name so it can work
    // with any country returned by ICountryService — no extra geocoding step needed.
    public interface IWeatherService
    {
        // Returns null if the API key is missing or the external call fails.
        Task<WeatherInfo?> GetWeatherAsync(double latitude, double longitude);
    }
}
