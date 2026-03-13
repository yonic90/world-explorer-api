using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    public interface IWeatherService
    {
        Task<WeatherInfo?> GetWeatherAsync(double latitude, double longitude);
    }
}
