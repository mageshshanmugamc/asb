namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _policyRepoMock = new Mock<IPolicyRepository>();
        _service = new PolicyService(_policyRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPolicies()
    {
        var policies = new List<Policy>
        {
            new() { Id = 1, Name = "FullAccess", Description = "Full", Resource = "*", Action = "*" },
            new() { Id = 2, Name = "ReadOnly", Description = "Read", Resource = "*", Action = "Read" }
        };

        _policyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(policies);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("FullAccess", result[0].Name);
        Assert.Equal("ReadOnly", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        _policyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Policy>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var policy = new Policy { Id = 1, Name = "ManageUsers", Description = "Manage", Resource = "User", Action = "Write" };

        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("ManageUsers", result!.Name);
        Assert.Equal("User", result.Resource);
        Assert.Equal("Write", result.Action);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _policyRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Policy?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedPolicy()
    {
        var dto = new CreatePolicyDto { Name = "NewPolicy", Description = "Desc", Resource = "Invoice", Action = "Read" };

        _policyRepoMock
            .Setup(r => r.CreateAsync(It.Is<Policy>(p => p.Name == "NewPolicy" && p.Resource == "Invoice")))
            .ReturnsAsync(new Policy { Id = 5, Name = "NewPolicy", Description = "Desc", Resource = "Invoice", Action = "Read" });

        var result = await _service.CreateAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("NewPolicy", result.Name);
        Assert.Equal("Invoice", result.Resource);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsUpdatedPolicy()
    {
        var dto = new UpdatePolicyDto { Name = "Updated", Description = "Updated desc", Resource = "Report", Action = "Delete" };

        _policyRepoMock
            .Setup(r => r.UpdateAsync(It.Is<Policy>(p => p.Id == 3 && p.Name == "Updated")))
            .ReturnsAsync(new Policy { Id = 3, Name = "Updated", Description = "Updated desc", Resource = "Report", Action = "Delete" });

        var result = await _service.UpdateAsync(3, dto);

        Assert.Equal(3, result.Id);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("Report", result.Resource);
        Assert.Equal("Delete", result.Action);
    }

    [Fact]
    public async Task CreateAsync_MapsAllFieldsToEntity()
    {
        var dto = new CreatePolicyDto { Name = "N", Description = "D", Resource = "R", Action = "A" };

        _policyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Policy>()))
            .ReturnsAsync(new Policy { Id = 1, Name = "N", Description = "D", Resource = "R", Action = "A" });

        await _service.CreateAsync(dto);

        _policyRepoMock.Verify(r => r.CreateAsync(It.Is<Policy>(p =>
            p.Name == "N" && p.Description == "D" && p.Resource == "R" && p.Action == "A")),
            Times.Once);
    }
}
