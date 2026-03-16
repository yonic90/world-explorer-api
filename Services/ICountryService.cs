using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    // Defining a service as an interface (contract) rather than a concrete class
    // is a core principle of Dependency Injection and SOLID design.
    //
    // Benefits:
    //   - The rest of the app depends on the interface, not the implementation.
    //   - You can swap CountryService for a mock/fake in tests without changing
    //     any endpoint code.
    //   - Makes dependencies explicit and easy to reason about.
    public interface ICountryService
    {
        // Returns null if the country was not found or the external API call failed.
        // The '?' on CountryInfo? is a nullable return type (C# 8+ feature).
        Task<CountryInfo?> GetCountryAsync(string name);
    }
}
