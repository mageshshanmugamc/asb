namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class PolicyControllerTests
{
    private readonly Mock<IPolicyService> _policyServiceMock;
    private readonly PolicyController _controller;

    public PolicyControllerTests()
    {
        _policyServiceMock = new Mock<IPolicyService>();
        _controller = new PolicyController(_policyServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPolicies()
    {
        var policies = new List<PolicyDto>
        {
            new() { Id = 1, Name = "FullAccess", Description = "Full access", Resource = "*", Action = "*" },
            new() { Id = 2, Name = "ReadOnly", Description = "Read only", Resource = "*", Action = "Read" }
        };

        _policyServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(policies);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PolicyDto>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
    {
        _policyServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<PolicyDto>());

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PolicyDto>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var policy = new PolicyDto { Id = 1, Name = "FullAccess", Description = "Full access", Resource = "*", Action = "*" };

        _policyServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(policy);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PolicyDto>(okResult.Value);
        Assert.Equal("FullAccess", returned.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _policyServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((PolicyDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new CreatePolicyRequest { Name = "WriteOnly", Description = "Write access", Resource = "Invoice", Action = "Write" };
        var created = new PolicyDto { Id = 5, Name = "WriteOnly", Description = "Write access", Resource = "Invoice", Action = "Write" };

        _policyServiceMock
            .Setup(s => s.CreateAsync(It.Is<CreatePolicyDto>(d => d.Name == "WriteOnly")))
            .ReturnsAsync(created);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        var returned = Assert.IsType<PolicyDto>(createdResult.Value);
        Assert.Equal("WriteOnly", returned.Name);
    }

    [Fact]
    public async Task Create_PassesCorrectDto()
    {
        var request = new CreatePolicyRequest { Name = "New", Description = "Desc", Resource = "Res", Action = "Act" };

        _policyServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreatePolicyDto>()))
            .ReturnsAsync(new PolicyDto { Id = 10, Name = "New", Description = "Desc", Resource = "Res", Action = "Act" });

        await _controller.Create(request);

        _policyServiceMock.Verify(
            s => s.CreateAsync(It.Is<CreatePolicyDto>(d =>
                d.Name == "New" && d.Description == "Desc" && d.Resource == "Res" && d.Action == "Act")),
            Times.Once);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var request = new UpdatePolicyRequest { Name = "Updated", Description = "Updated desc", Resource = "User", Action = "Delete" };
        var updated = new PolicyDto { Id = 1, Name = "Updated", Description = "Updated desc", Resource = "User", Action = "Delete" };

        _policyServiceMock
            .Setup(s => s.UpdateAsync(1, It.IsAny<UpdatePolicyDto>()))
            .ReturnsAsync(updated);

        var result = await _controller.Update(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PolicyDto>(okResult.Value);
        Assert.Equal("Updated", returned.Name);
    }

    [Fact]
    public async Task Update_PassesCorrectIdAndDto()
    {
        var request = new UpdatePolicyRequest { Name = "Renamed", Description = "D", Resource = "R", Action = "A" };

        _policyServiceMock
            .Setup(s => s.UpdateAsync(7, It.IsAny<UpdatePolicyDto>()))
            .ReturnsAsync(new PolicyDto { Id = 7, Name = "Renamed", Description = "D", Resource = "R", Action = "A" });

        await _controller.Update(7, request);

        _policyServiceMock.Verify(
            s => s.UpdateAsync(7, It.Is<UpdatePolicyDto>(d =>
                d.Name == "Renamed" && d.Resource == "R")),
            Times.Once);
    }
}
