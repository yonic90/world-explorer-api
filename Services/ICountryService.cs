using WorldExplorerApi.Models;

namespace WorldExplorerApi.Services
{
    public interface ICountryService
    {
        Task<CountryInfo?> GetCountryAsync(string name);
    }
}
