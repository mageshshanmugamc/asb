namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Admin.v1.Response;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class RoleControllerTests
{
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly RoleController _controller;

    public RoleControllerTests()
    {
        _roleServiceMock = new Mock<IRoleService>();
        _controller = new RoleController(_roleServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithRoles()
    {
        var roles = new List<RoleDto>
        {
            new() { Id = 1, Name = "Admin" },
            new() { Id = 2, Name = "Viewer" }
        };

        _roleServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(roles);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleResponse>>(okResult.Value);
        Assert.Equal(2, returnedRoles.Count());
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
    {
        _roleServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<RoleDto>());

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleResponse>>(okResult.Value);
        Assert.Empty(returnedRoles);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var role = new RoleDto { Id = 1, Name = "Admin" };

        _roleServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(role);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRole = Assert.IsType<RoleResponse>(okResult.Value);
        Assert.Equal("Admin", returnedRole.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _roleServiceMock
            .Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((RoleDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new CreateRoleRequest { Name = "Editor" };
        var createdRole = new RoleDto { Id = 3, Name = "Editor" };

        _roleServiceMock
            .Setup(s => s.CreateAsync(It.Is<CreateRoleDto>(d => d.Name == "Editor")))
            .ReturnsAsync(createdRole);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedRole = Assert.IsType<RoleResponse>(createdResult.Value);
        Assert.Equal("Editor", returnedRole.Name);
    }

    [Fact]
    public async Task Create_ValidRequest_PassesCorrectDto()
    {
        var request = new CreateRoleRequest { Name = "NewRole" };

        _roleServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateRoleDto>()))
            .ReturnsAsync(new RoleDto { Id = 5, Name = "NewRole" });

        await _controller.Create(request);

        _roleServiceMock.Verify(
            s => s.CreateAsync(It.Is<CreateRoleDto>(d => d.Name == "NewRole")),
            Times.Once);
    }

    [Fact]
    public async Task AssignPolicyToRole_ValidIds_ReturnsNoContent()
    {
        _roleServiceMock
            .Setup(s => s.AssignPolicyToRoleAsync(2, 10))
            .Returns(Task.CompletedTask);

        var result = await _controller.AssignPolicyToRole(2, 10);

        Assert.IsType<NoContentResult>(result);
        _roleServiceMock.Verify(s => s.AssignPolicyToRoleAsync(2, 10), Times.Once);
    }
}
