namespace ASB.Services.v1.Interfaces
{
    using ASB.Services.v1.Dtos;

    /// <summary>
    /// Interface for country-related operations. This service is responsible for retrieving country information from the database or any other data source. It abstracts the data access layer and provides a clean API for the controllers to interact with when they need to retrieve country data.
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all countries. This method is used by the controllers to get the necessary data to populate dropdowns or selection lists in the UI where users need to select a country.
        /// </summary>
        /// <returns>A list of CountryDto objects representing the countries.</returns>
        Task<List<CountryDto>> GetCountriesAsync();

        Task<CountryDto?> GetCountryByIdAsync(int id);

        Task<CountryDto> CreateCountryAsync(CountryDto dto);

        Task<CountryDto?> UpdateCountryAsync(CountryDto dto);

        Task<bool> DeleteCountryAsync(int id);
    }
}