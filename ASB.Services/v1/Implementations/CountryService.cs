namespace ASB.Services.v1.Implementations
{
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using ASB.Repositories.v1.Interfaces;
    using ASB.Repositories.v1.Entities;

    /// <summary>
    /// Implementation of the ICountryService interface. This service is responsible for retrieving country information from the database or any other data source. It abstracts the data access layer and provides a clean API for the controllers to interact with when they need to retrieve country data.
    /// </summary>
    /// <param name="countryRepository">The repository used to access country data.</param>
    public class CountryService(ICountryRepository countryRepository) : ICountryService
    {
        private readonly ICountryRepository _countryRepository = countryRepository;

        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            var countries = await _countryRepository.GetAllAsync();
            return countries.Select(c => new CountryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Market = c.Market
            }).ToList();
        }

        public async Task<CountryDto?> GetCountryByIdAsync(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country is null) return null;

            return new CountryDto
            {
                Id = country.Id,
                Name = country.Name,
                Code = country.Code,
                Market = country.Market
            };
        }

        public async Task<CountryDto> CreateCountryAsync(CountryDto dto)
        {
            var entity = new Country
            {
                Name = dto.Name,
                Code = dto.Code,
                Market = dto.Market
            };

            var created = await _countryRepository.CreateAsync(entity);
            return new CountryDto
            {
                Id = created.Id,
                Name = created.Name,
                Code = created.Code,
                Market = created.Market
            };
        }

        public async Task<CountryDto?> UpdateCountryAsync(CountryDto dto)
        {
            var entity = new Country
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Market = dto.Market
            };

            var updated = await _countryRepository.UpdateAsync(entity);
            if (updated is null) return null;

            return new CountryDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Code = updated.Code,
                Market = updated.Market
            };
        }

        public async Task<bool> DeleteCountryAsync(int id)
        {
            return await _countryRepository.DeleteAsync(id);
        }
    }
}