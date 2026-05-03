namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Infrastructure;
using ASB.Admin.v1.Requests;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IKeycloakAdminService> _keycloakAdminServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _keycloakAdminServiceMock = new Mock<IKeycloakAdminService>();
        _controller = new UserController(_userServiceMock.Object, _keycloakAdminServiceMock.Object);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupId = 2
        };

        var createdUser = new UserDto
        {
            Id = 1,
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupId = 2
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(createdUser);

        var result = await _controller.CreateUser(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_NoGroupId_DefaultsToGroup4()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupId = null
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupId == 4)))
            .ReturnsAsync(new UserDto { Id = 1, Username = "newuser", Email = "newuser@example.com" });

        await _controller.CreateUser(request);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupId == 4)),
            Times.Once);
    }

    [Fact]
    public async Task CreateUser_WithGroupId_UsesProvidedGroupId()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupId = 7
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupId == 7)))
            .ReturnsAsync(new UserDto { Id = 1, Username = "newuser", Email = "newuser@example.com" });

        await _controller.CreateUser(request);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupId == 7)),
            Times.Once);
    }

    [Fact]
    public async Task AddUserToGroup_ValidIds_ReturnsNoContent()
    {
        _userServiceMock
            .Setup(s => s.AddUserToGroupAsync(1, 2))
            .Returns(Task.CompletedTask);

        var result = await _controller.AddUserToGroup(1, 2);

        Assert.IsType<NoContentResult>(result);
        _userServiceMock.Verify(s => s.AddUserToGroupAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task LookupKeycloakUser_EmptyEmail_ReturnsBadRequest()
    {
        var result = await _controller.LookupKeycloakUser("");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task LookupKeycloakUser_NullEmail_ReturnsBadRequest()
    {
        var result = await _controller.LookupKeycloakUser(null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task LookupKeycloakUser_UserNotFound_ReturnsNotFound()
    {
        _keycloakAdminServiceMock
            .Setup(s => s.GetUserByEmailAsync("notfound@example.com"))
            .ReturnsAsync((KeycloakUserInfo?)null);

        var result = await _controller.LookupKeycloakUser("notfound@example.com");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task LookupKeycloakUser_UserFound_ReturnsOk()
    {
        var kcUser = new KeycloakUserInfo
        {
            Id = "kc-123",
            Username = "kcuser",
            Email = "kcuser@example.com",
            FirstName = "KC",
            LastName = "User"
        };

        _keycloakAdminServiceMock
            .Setup(s => s.GetUserByEmailAsync("kcuser@example.com"))
            .ReturnsAsync(kcUser);

        var result = await _controller.LookupKeycloakUser("kcuser@example.com");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task LookupKeycloakUser_WhitespaceEmail_ReturnsBadRequest()
    {
        var result = await _controller.LookupKeycloakUser("   ");

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
