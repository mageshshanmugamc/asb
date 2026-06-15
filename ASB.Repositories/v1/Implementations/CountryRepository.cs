namespace ASB.Repositories.v1.Implementations
{
    using ASB.Repositories.v1.Entities;
    using ASB.Repositories.v1.Contexts;
    using ASB.Repositories.v1.Interfaces;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Implementation of the ICountryRepository interface for country-related data access operations. This repository is responsible for interacting with the database or any other data source to retrieve country information. It abstracts the underlying data access implementation and provides a clean API for the services to interact with when they need to retrieve country data.
    /// </summary>
    public class CountryRepository(AsbContext dbContext) : ICountryRepository, IDisposable
    {
        private readonly AsbContext _dbContext = dbContext;

        /// <summary>
        /// Asynchronously retrieves a list of all countries from the data source. This method is used by the services to get the necessary data to populate dropdowns or selection lists in the UI where users need to select a country.
        /// </summary>
        /// <returns>A list of Country entities representing the countries.</returns>
        public async Task<List<Country>> GetAllAsync()
        {
            return await _dbContext.Countries.ToListAsync();
        }

        public async Task<Country?> GetByIdAsync(int id)
        {
            return await _dbContext.Countries.FindAsync(id);
        }

        public async Task<Country> CreateAsync(Country country)
        {
            _dbContext.Countries.Add(country);
            await _dbContext.SaveChangesAsync();
            return country;
        }

        public async Task<Country?> UpdateAsync(Country country)
        {
            var existing = await _dbContext.Countries.FindAsync(country.Id);
            if (existing is null) return null;

            existing.Name = country.Name;
            existing.Code = country.Code;
            existing.Market = country.Market;
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var country = await _dbContext.Countries.FindAsync(id);
            if (country is null) return false;

            _dbContext.Countries.Remove(country);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Disposes the database context resource.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected virtual method for disposing resources.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(); false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext?.Dispose();
            }
        }
    }
}