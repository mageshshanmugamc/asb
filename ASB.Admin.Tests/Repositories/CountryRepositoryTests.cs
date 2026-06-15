using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASB.Admin.Tests.Repositories;

public class CountryRepositoryTests : IDisposable
{
    private readonly AsbContext _context;
    private readonly CountryRepository _repository;

    public CountryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AsbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AsbContext(options);
        _repository = new CountryRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCountries()
    {
        _context.Countries.AddRange(
            new Country { Id = 1, Name = "United States", Code = "US", Market = "North America" },
            new Country { Id = 2, Name = "Canada", Code = "CA", Market = "North America" }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCountry()
    {
        _context.Countries.Add(new Country { Id = 1, Name = "United States", Code = "US", Market = "North America" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("United States", result.Name);
        Assert.Equal("US", result.Code);
        Assert.Equal("North America", result.Market);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsCountryToDatabase()
    {
        var country = new Country { Name = "Germany", Code = "DE", Market = "Europe" };

        var result = await _repository.CreateAsync(country);

        Assert.True(result.Id > 0);
        Assert.Equal("Germany", result.Name);
        Assert.Equal("DE", result.Code);
        Assert.Equal("Europe", result.Market);

        var dbCountry = await _context.Countries.FindAsync(result.Id);
        Assert.NotNull(dbCountry);
        Assert.Equal("Germany", dbCountry.Name);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCountry_UpdatesAndReturnsCountry()
    {
        _context.Countries.Add(new Country { Id = 1, Name = "United States", Code = "US", Market = "North America" });
        await _context.SaveChangesAsync();

        var updated = new Country { Id = 1, Name = "USA", Code = "USA", Market = "Americas" };

        var result = await _repository.UpdateAsync(updated);

        Assert.NotNull(result);
        Assert.Equal("USA", result.Name);
        Assert.Equal("USA", result.Code);
        Assert.Equal("Americas", result.Market);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCountry_ReturnsNull()
    {
        var updated = new Country { Id = 999, Name = "Unknown", Code = "XX", Market = "None" };

        var result = await _repository.UpdateAsync(updated);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCountry_ReturnsTrueAndRemoves()
    {
        _context.Countries.Add(new Country { Id = 1, Name = "United States", Code = "US", Market = "North America" });
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(1);

        Assert.True(result);
        Assert.Null(await _context.Countries.FindAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingCountry_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.False(result);
    }
}
