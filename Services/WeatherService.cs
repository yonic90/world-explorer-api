using System.Text.Json;
using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? "";
        }

        public async Task<WeatherInfo?> GetWeatherAsync(double latitude, double longitude)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return null;

            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var weather = root.GetProperty("weather")[0];
            var main = root.GetProperty("main");

            return new WeatherInfo
            {
                Description = weather.GetProperty("description").GetString() ?? "",
                Temperature = main.GetProperty("temp").GetDouble(),
                FeelsLike = main.GetProperty("feels_like").GetDouble(),
                Humidity = main.GetProperty("humidity").GetInt32(),
                Icon = weather.GetProperty("icon").GetString() ?? ""
            };
        }
    }
}
