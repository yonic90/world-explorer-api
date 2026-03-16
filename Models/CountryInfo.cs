namespace WorldExplorerApi.Models
{
    // CountryInfo is a DTO (Data Transfer Object) — a simple class whose only
    // job is to carry data between layers of the application.
    //
    // DTOs are intentionally "dumb" (no business logic, no methods).
    // They define exactly what the API sends to the client, independent of
    // the external API's JSON structure (which CountryService handles).
    //
    // ASP.NET Core automatically serialises this class to JSON when returned
    // from an endpoint via Results.Ok(). Property names become camelCase by default
    // (e.g. "Name" → "name", "FeelsLike" → "feelsLike").
    public class CountryInfo
    {
        public string Name { get; set; }        // Common name, e.g. "Germany"
        public string Capital { get; set; }     // Capital city, e.g. "Berlin"
        public string Flag { get; set; }        // URL to the flag PNG image
        public long Population { get; set; }    // Total population (long for large countries)
        public string Region { get; set; }      // Geographic region, e.g. "Europe"
        public string Currency { get; set; }    // Currency name(s), e.g. "Euro"
        public string Language { get; set; }    // Official language(s), e.g. "German"

        // Coordinates are needed to fetch weather data.
        // They are included in the API response but the frontend typically won't display them.
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
