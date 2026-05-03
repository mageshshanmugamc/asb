namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "u1@test.com", UserGroupMappings = [new UserGroupMapping { UserGroupId = 1 }, new UserGroupMapping { UserGroupId = 3 }] },
            new() { Id = 2, Username = "user2", Email = "u2@test.com", UserGroupMappings = [] }
        };

        _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        var result = (await _service.GetUsers()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("user1", result[0].Username);
        Assert.Equal(2, result[0].UserGroupIds.Count);
        Assert.Contains(1, result[0].UserGroupIds);
        Assert.Contains(3, result[0].UserGroupIds);
        Assert.Empty(result[1].UserGroupIds);
    }

    [Fact]
    public async Task GetUsers_EmptyList_ReturnsEmpty()
    {
        _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

        var result = await _service.GetUsers();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateUserAsync_ValidDto_ReturnsCreatedUser()
    {
        var dto = new CreateUserDto { Username = "newuser", Email = "new@test.com", UserGroupIds = [3] };

        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = 10, Username = "newuser", Email = "new@test.com" });
        _userRepositoryMock.Setup(r => r.AddUserToGroupAsync(10, 3, null)).Returns(Task.CompletedTask);

        var result = await _service.CreateUserAsync(dto);

        Assert.Equal(10, result.Id);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("new@test.com", result.Email);
        Assert.Single(result.UserGroupIds);
        Assert.Contains(3, result.UserGroupIds);
    }

    [Fact]
    public async Task CreateUserAsync_MultipleGroups_CallsAddForEach()
    {
        var dto = new CreateUserDto { Username = "u", Email = "u@t.com", UserGroupIds = [2, 5, 8] };

        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("u@t.com")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = 20, Username = "u", Email = "u@t.com" });
        _userRepositoryMock.Setup(r => r.AddUserToGroupAsync(It.IsAny<int>(), It.IsAny<int>(), null)).Returns(Task.CompletedTask);

        await _service.CreateUserAsync(dto);

        _userRepositoryMock.Verify(r => r.AddUserToGroupAsync(20, 2, null), Times.Once);
        _userRepositoryMock.Verify(r => r.AddUserToGroupAsync(20, 5, null), Times.Once);
        _userRepositoryMock.Verify(r => r.AddUserToGroupAsync(20, 8, null), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_EmptyGroupIds_DoesNotCallAddToGroup()
    {
        var dto = new CreateUserDto { Username = "u", Email = "u@t.com", UserGroupIds = [] };

        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("u@t.com")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = 30, Username = "u", Email = "u@t.com" });

        await _service.CreateUserAsync(dto);

        _userRepositoryMock.Verify(r => r.AddUserToGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        var dto = new CreateUserDto { Username = "dup", Email = "exists@test.com", UserGroupIds = [1] };

        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("exists@test.com"))
            .ReturnsAsync(new User { Id = 5, Username = "existing", Email = "exists@test.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(dto));
    }

    [Fact]
    public async Task AddUserToGroupAsync_UserExists_CallsRepository()
    {
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Username = "u", Email = "u@t.com" });
        _userRepositoryMock.Setup(r => r.AddUserToGroupAsync(1, 5, null)).Returns(Task.CompletedTask);

        await _service.AddUserToGroupAsync(1, 5);

        _userRepositoryMock.Verify(r => r.AddUserToGroupAsync(1, 5, null), Times.Once);
    }

    [Fact]
    public async Task AddUserToGroupAsync_UserNotFound_ThrowsKeyNotFound()
    {
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddUserToGroupAsync(999, 1));
    }
}
