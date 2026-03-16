using System.Text.Json;
using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    // CountryService fetches and parses country data from the free RestCountries API.
    // API docs: https://restcountries.com
    // Example response: https://restcountries.com/v3.1/name/portugal
    public class CountryService : ICountryService
    {
        // IHttpClientFactory is injected by the DI container (registered in Program.cs).
        // We store it as a field so CreateClient() can be called per-request.
        // Using IHttpClientFactory (instead of `new HttpClient()`) is important because
        // it reuses underlying TCP connections and avoids socket exhaustion.
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor injection — ASP.NET Core's DI container calls this automatically
        // and supplies the registered IHttpClientFactory implementation.
        public CountryService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // async/await is used throughout because HTTP calls are I/O-bound operations.
        // Using async means the thread is freed while waiting for the network response,
        // allowing the server to handle other requests in the meantime.
        public async Task<CountryInfo?> GetCountryAsync(string name)
        {
            var client = _httpClientFactory.CreateClient();

            // Uri.EscapeDataString encodes special characters (spaces, accents, etc.)
            // so the country name is safe to include in a URL.
            // e.g. "United States" → "United%20States"
            var response = await client.GetAsync($"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(name)}");

            // If the API returns a non-2xx status (e.g. 404 for unknown country),
            // we return null and let the endpoint return a 404 to the client.
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            // JsonDocument is a lightweight, read-only DOM for parsing JSON.
            // 'using' ensures the unmanaged memory is released when we're done.
            // The RestCountries API always returns an array, even for a single match.
            using var doc = JsonDocument.Parse(json);
            var country = doc.RootElement[0]; // Take the first (best) match

            // TryGetProperty is used for fields that may be absent in the JSON.
            // The capital is an array (some countries have multiple capitals),
            // so we take only the first entry.
            var capital = country.TryGetProperty("capital", out var capitalArr) && capitalArr.GetArrayLength() > 0
                ? capitalArr[0].GetString() ?? ""
                : "";

            // currencies is a nested object keyed by currency code, e.g.:
            // "currencies": { "EUR": { "name": "Euro", "symbol": "€" } }
            // We enumerate the properties and join the currency names into a string.
            var currencies = country.TryGetProperty("currencies", out var curr)
                ? string.Join(", ", curr.EnumerateObject().Select(c =>
                    c.Value.TryGetProperty("name", out var n) ? n.GetString() ?? c.Name : c.Name))
                : "";

            // languages is also a nested object, e.g.:
            // "languages": { "por": "Portuguese" }
            // We take the values (language names) and join them.
            var languages = country.TryGetProperty("languages", out var langs)
                ? string.Join(", ", langs.EnumerateObject().Select(l => l.Value.GetString() ?? l.Name))
                : "";

            // latlng is a two-element array: [latitude, longitude].
            // We use a C# value tuple to hold both values together.
            var latlng = country.TryGetProperty("latlng", out var ll) && ll.GetArrayLength() >= 2
                ? (ll[0].GetDouble(), ll[1].GetDouble())
                : (0.0, 0.0);

            // Build and return the CountryInfo model (our own DTO, not the raw API response).
            // Mapping to a DTO decouples the rest of the app from the external API's shape —
            // if RestCountries changes its JSON structure, we only update this one place.
            return new CountryInfo
            {
                // name.common is the everyday name (e.g. "Germany" vs. "Federal Republic of Germany")
                Name = country.GetProperty("name").GetProperty("common").GetString() ?? name,
                Capital = capital,
                // flags.png is a URL to the country's flag image
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
