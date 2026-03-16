namespace WorldExplorerApi.Models
{
    // WeatherInfo is a DTO representing the current weather at a location.
    // It is a subset of the OpenWeatherMap API response — only the fields
    // our frontend needs, mapped to cleaner property names.
    public class WeatherInfo
    {
        public string Description { get; set; }  // Human-readable condition, e.g. "light rain"
        public double Temperature { get; set; }  // Current temperature in °C
        public double FeelsLike { get; set; }    // Perceived temperature in °C
        public int Humidity { get; set; }        // Relative humidity as a percentage (0–100)

        // Icon code from OpenWeatherMap, e.g. "01d" (clear sky, day) or "10n" (rain, night).
        // Build the image URL as: https://openweathermap.org/img/wn/{Icon}@2x.png
        public string Icon { get; set; }
    }
}
