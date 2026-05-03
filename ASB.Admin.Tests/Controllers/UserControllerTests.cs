namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Admin.v1.Response;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupIds = [2]
        };

        var createdUser = new UserDto
        {
            Id = 1,
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupIds = [2]
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(createdUser);

        var result = await _controller.CreateUser(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_NoGroupIds_PassesEmptyList()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com"
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupIds.Count == 0)))
            .ReturnsAsync(new UserDto { Id = 1, Username = "newuser", Email = "newuser@example.com" });

        await _controller.CreateUser(request);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupIds.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task CreateUser_WithMultipleGroupIds_PassesAllGroupIds()
    {
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            UserGroupIds = [2, 5, 7]
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.UserGroupIds.Count == 3)))
            .ReturnsAsync(new UserDto { Id = 1, Username = "newuser", Email = "newuser@example.com", UserGroupIds = [2, 5, 7] });

        await _controller.CreateUser(request);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(It.Is<CreateUserDto>(d =>
                d.UserGroupIds.Contains(2) && d.UserGroupIds.Contains(5) && d.UserGroupIds.Contains(7))),
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
}
