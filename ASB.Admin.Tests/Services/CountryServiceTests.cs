using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

namespace ASB.Admin.Tests.Services;

public class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _countryRepositoryMock;
    private readonly CountryService _service;

    public CountryServiceTests()
    {
        _countryRepositoryMock = new Mock<ICountryRepository>();
        _service = new CountryService(_countryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCountriesAsync_ReturnsMappedDtos()
    {
        var countries = new List<Country>
        {
            new() { Id = 1, Name = "United States", Code = "US", Market = "North America" },
            new() { Id = 2, Name = "Canada", Code = "CA", Market = "North America" }
        };
        _countryRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(countries);

        var result = await _service.GetCountriesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("United States", result[0].Name);
        Assert.Equal("US", result[0].Code);
        Assert.Equal("Canada", result[1].Name);
    }

    [Fact]
    public async Task GetCountriesAsync_EmptyList_ReturnsEmptyList()
    {
        _countryRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.GetCountriesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCountryByIdAsync_Exists_ReturnsMappedDto()
    {
        var country = new Country { Id = 1, Name = "United States", Code = "US", Market = "North America" };
        _countryRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(country);

        var result = await _service.GetCountryByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("United States", result.Name);
        Assert.Equal("US", result.Code);
        Assert.Equal("North America", result.Market);
    }

    [Fact]
    public async Task GetCountryByIdAsync_NotFound_ReturnsNull()
    {
        _countryRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Country?)null);

        var result = await _service.GetCountryByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCountryAsync_CallsRepositoryAndReturnsMappedDto()
    {
        var created = new Country { Id = 10, Name = "Germany", Code = "DE", Market = "Europe" };
        _countryRepositoryMock
            .Setup(r => r.CreateAsync(It.Is<Country>(c => c.Name == "Germany" && c.Code == "DE" && c.Market == "Europe")))
            .ReturnsAsync(created);

        var dto = new CountryDto { Name = "Germany", Code = "DE", Market = "Europe" };
        var result = await _service.CreateCountryAsync(dto);

        Assert.Equal(10, result.Id);
        Assert.Equal("Germany", result.Name);
        Assert.Equal("DE", result.Code);
        Assert.Equal("Europe", result.Market);
        _countryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Country>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCountryAsync_Exists_ReturnsUpdatedDto()
    {
        var updated = new Country { Id = 1, Name = "USA", Code = "USA", Market = "Americas" };
        _countryRepositoryMock
            .Setup(r => r.UpdateAsync(It.Is<Country>(c => c.Id == 1)))
            .ReturnsAsync(updated);

        var dto = new CountryDto { Id = 1, Name = "USA", Code = "USA", Market = "Americas" };
        var result = await _service.UpdateCountryAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("USA", result.Name);
        Assert.Equal("USA", result.Code);
        Assert.Equal("Americas", result.Market);
        _countryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Country>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCountryAsync_NotFound_ReturnsNull()
    {
        _countryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Country>()))
            .ReturnsAsync((Country?)null);

        var dto = new CountryDto { Id = 999, Name = "Unknown", Code = "XX", Market = "None" };
        var result = await _service.UpdateCountryAsync(dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCountryAsync_Exists_ReturnsTrue()
    {
        _countryRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteCountryAsync(1);

        Assert.True(result);
        _countryRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCountryAsync_NotFound_ReturnsFalse()
    {
        _countryRepositoryMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _service.DeleteCountryAsync(999);

        Assert.False(result);
    }
}
