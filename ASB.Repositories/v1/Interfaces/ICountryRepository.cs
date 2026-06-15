namespace ASB.Repositories.v1.Interfaces
{
    using ASB.Repositories.v1.Entities;

    /// <summary>
    /// Interface for country-related data access operations. This repository is responsible for interacting with the database or any other data source to retrieve country information. It abstracts the underlying data access implementation and provides a clean API for the services to interact with when they need to retrieve country data.
    /// </summary>
    public interface ICountryRepository
    {
        /// <summary>
        /// Asynchronously retrieves a list of all countries from the data source. This method is used by the services to get the necessary data to populate dropdowns or selection lists in the UI where users need to select a country.
        /// </summary>
        /// <returns>A list of Country entities representing the countries.</returns>
        Task<List<Country>> GetAllAsync();

        Task<Country?> GetByIdAsync(int id);

        Task<Country> CreateAsync(Country country);

        Task<Country?> UpdateAsync(Country country);

        Task<bool> DeleteAsync(int id);
    }
}