using System.Text.Json;
using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    public class CountryService : ICountryService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CountryService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CountryInfo?> GetCountryAsync(string name)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(name)}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var country = doc.RootElement[0];

            var capital = country.TryGetProperty("capital", out var capitalArr) && capitalArr.GetArrayLength() > 0
                ? capitalArr[0].GetString() ?? ""
                : "";

            var currencies = country.TryGetProperty("currencies", out var curr)
                ? string.Join(", ", curr.EnumerateObject().Select(c =>
                    c.Value.TryGetProperty("name", out var n) ? n.GetString() ?? c.Name : c.Name))
                : "";

            var languages = country.TryGetProperty("languages", out var langs)
                ? string.Join(", ", langs.EnumerateObject().Select(l => l.Value.GetString() ?? l.Name))
                : "";

            var latlng = country.TryGetProperty("latlng", out var ll) && ll.GetArrayLength() >= 2
                ? (ll[0].GetDouble(), ll[1].GetDouble())
                : (0.0, 0.0);

            return new CountryInfo
            {
                Name = country.GetProperty("name").GetProperty("common").GetString() ?? name,
                Capital = capital,
                Flag = country.TryGetProperty("flags", out var flags) && 
       flags.TryGetProperty("png", out var png) 
       ? png.GetString() ?? "" 
       : "",
                Population = country.TryGetProperty("population", out var pop) ? pop.GetInt64() : 0,
                Region = country.TryGetProperty("region", out var region) ? region.GetString() ?? "" : "",
                Currency = currencies,
                Language = languages,
                Latitude = latlng.Item1,
                Longitude = latlng.Item2
            };
        }
    }
}
