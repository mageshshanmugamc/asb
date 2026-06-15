using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Admin.v1.Response;
using ASB.ErrorHandler.v1.Exceptions;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ASB.Admin.Tests.Controllers;

public class CountryControllerTests
{
    private readonly Mock<ICountryService> _countryServiceMock;
    private readonly CountryController _controller;

    public CountryControllerTests()
    {
        _countryServiceMock = new Mock<ICountryService>();
        _controller = new CountryController(_countryServiceMock.Object);
    }

    [Fact]
    public async Task GetCountries_ReturnsOkWithCountryList()
    {
        var countries = new List<CountryDto>
        {
            new() { Id = 1, Name = "United States", Code = "US", Market = "North America" },
            new() { Id = 2, Name = "Canada", Code = "CA", Market = "North America" }
        };
        _countryServiceMock.Setup(s => s.GetCountriesAsync()).ReturnsAsync(countries);

        var result = await _controller.GetCountries();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var responses = Assert.IsType<List<CountryResponse>>(okResult.Value);
        Assert.Equal(2, responses.Count);
        Assert.Equal("United States", responses[0].Name);
        Assert.Equal("US", responses[0].Code);
    }

    [Fact]
    public async Task GetCountries_EmptyList_ReturnsOkWithEmptyList()
    {
        _countryServiceMock.Setup(s => s.GetCountriesAsync()).ReturnsAsync([]);

        var result = await _controller.GetCountries();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var responses = Assert.IsType<List<CountryResponse>>(okResult.Value);
        Assert.Empty(responses);
    }

    [Fact]
    public async Task GetCountryById_Exists_ReturnsOkWithCountry()
    {
        var country = new CountryDto { Id = 1, Name = "United States", Code = "US", Market = "North America" };
        _countryServiceMock.Setup(s => s.GetCountryByIdAsync(1)).ReturnsAsync(country);

        var result = await _controller.GetCountryById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CountryResponse>(okResult.Value);
        Assert.Equal(1, response.Id);
        Assert.Equal("United States", response.Name);
        Assert.Equal("US", response.Code);
        Assert.Equal("North America", response.Market);
    }

    [Fact]
    public async Task GetCountryById_NotFound_ReturnsNotFound()
    {
        _countryServiceMock.Setup(s => s.GetCountryByIdAsync(999)).ReturnsAsync((CountryDto?)null);

        var result = await _controller.GetCountryById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateCountry_ReturnsCreatedAtAction()
    {
        var created = new CountryDto { Id = 10, Name = "Germany", Code = "DE", Market = "Europe" };
        _countryServiceMock
            .Setup(s => s.CreateCountryAsync(It.Is<CountryDto>(d => d.Name == "Germany" && d.Code == "DE" && d.Market == "Europe")))
            .ReturnsAsync(created);

        var request = new CreateCountryRequest { Name = "Germany", Code = "DE", Market = "Europe" };
        var result = await _controller.CreateCountry(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(CountryController.GetCountryById), createdResult.ActionName);

        var response = Assert.IsType<CountryResponse>(createdResult.Value);
        Assert.Equal(10, response.Id);
        Assert.Equal("Germany", response.Name);
        Assert.Equal("DE", response.Code);
        Assert.Equal("Europe", response.Market);
    }

    [Fact]
    public async Task UpdateCountry_Exists_ReturnsOkWithUpdatedCountry()
    {
        var updated = new CountryDto { Id = 1, Name = "USA", Code = "USA", Market = "Americas" };
        _countryServiceMock
            .Setup(s => s.UpdateCountryAsync(It.Is<CountryDto>(d => d.Id == 1)))
            .ReturnsAsync(updated);

        var request = new UpdateCountryRequest { Name = "USA", Code = "USA", Market = "Americas" };
        var result = await _controller.UpdateCountry(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CountryResponse>(okResult.Value);
        Assert.Equal("USA", response.Name);
        Assert.Equal("USA", response.Code);
        Assert.Equal("Americas", response.Market);
    }

    [Fact]
    public async Task UpdateCountry_NotFound_ThrowsNotFoundException()
    {
        _countryServiceMock
            .Setup(s => s.UpdateCountryAsync(It.IsAny<CountryDto>()))
            .ReturnsAsync((CountryDto?)null);

        var request = new UpdateCountryRequest { Name = "Unknown", Code = "XX", Market = "None" };

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.UpdateCountry(999, request));
    }

    [Fact]
    public async Task DeleteCountry_Exists_ReturnsNoContent()
    {
        _countryServiceMock.Setup(s => s.DeleteCountryAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteCountry(1);

        Assert.IsType<NoContentResult>(result);
        _countryServiceMock.Verify(s => s.DeleteCountryAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCountry_NotFound_ThrowsNotFoundException()
    {
        _countryServiceMock.Setup(s => s.DeleteCountryAsync(999)).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.DeleteCountry(999));
    }
}
