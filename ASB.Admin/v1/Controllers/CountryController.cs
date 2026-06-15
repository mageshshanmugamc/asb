namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using ASB.ErrorHandler.v1.Exceptions;

    [ApiController]
    [Route("api/v1/countries")]
    [AsbAuthorize]
    public class CountryController(ICountryService countryService) : ControllerBase
    {
        private readonly ICountryService countryService = countryService;

        [HttpGet]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<ActionResult<List<CountryResponse>>> GetCountries()
        {
            var countries = await countryService.GetCountriesAsync();
            var countryResponses = countries.Select(c => new CountryResponse
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Market = c.Market
            }).ToList();
            return Ok(countryResponses);
        }

        [HttpGet("{id}")]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetCountryById(int id)
        {
            var country = await countryService.GetCountryByIdAsync(id);
            if (country is null)
                return NotFound();

            return Ok(new CountryResponse
            {
                Id = country.Id,
                Code = country.Code,
                Name = country.Name,
                Market = country.Market
            });
        }

        [HttpPost]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequest request)
        {
            var dto = new CountryDto
            {
                Name = request.Name,
                Code = request.Code,
                Market = request.Market
            };

            var created = await countryService.CreateCountryAsync(dto);
            return CreatedAtAction(nameof(GetCountryById), new { id = created.Id }, new CountryResponse
            {
                Id = created.Id,
                Code = created.Code,
                Name = created.Name,
                Market = created.Market
            });
        }

        [HttpPut("{id}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryRequest request)
        {
            var dto = new CountryDto
            {
                Id = id,
                Name = request.Name,
                Code = request.Code,
                Market = request.Market
            };

            var updated = await countryService.UpdateCountryAsync(dto);
            if (updated is null)
                throw new NotFoundException("Country not found.");

            return Ok(new CountryResponse
            {
                Id = updated.Id,
                Code = updated.Code,
                Name = updated.Name,
                Market = updated.Market
            });
        }

        [HttpDelete("{id}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var deleted = await countryService.DeleteCountryAsync(id);
            if (!deleted)
                throw new NotFoundException("Country not found.");

            return NoContent();
        }
    }
}