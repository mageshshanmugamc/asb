namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class AuthTokenServiceTests
{
    private readonly Mock<IMenuRepository> _menuRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthTokenService _service;

    public AuthTokenServiceTests()
    {
        _menuRepoMock = new Mock<IMenuRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(c => c["Jwt:Secret"]).Returns("ThisIsAVeryLongSecretKeyForTesting1234567890!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("test-audience");

        _service = new AuthTokenService(_menuRepoMock.Object, _userRepoMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task GenerateAppTokenAsync_UserNotFound_ThrowsKeyNotFound()
    {
        _userRepoMock.Setup(r => r.GetUserByEmailAsync("missing@test.com")).ReturnsAsync((User?)null);

        var dto = new GenerateTokenDto { Username = "missing", Email = "missing@test.com" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GenerateAppTokenAsync(dto));
    }

    [Fact]
    public async Task GenerateAppTokenAsync_ValidUser_ReturnsToken()
    {
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com" };
        var roleIds = new List<int> { 1 };
        var menus = new List<Menu>
        {
            new()
            {
                Id = 1, Name = "Dashboard", Route = "/dashboard", DisplayOrder = 1,
                RoleMenuPermissions = [new RoleMenuPermission { RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } }]
            }
        };

        _userRepoMock.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(1)).ReturnsAsync(roleIds);
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(roleIds)).ReturnsAsync(menus);

        var dto = new GenerateTokenDto { Username = "testuser", Email = "test@test.com" };
        var result = await _service.GenerateAppTokenAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Contains("Admin", result.Roles);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateAppTokenAsync_NoMenus_StillReturnsToken()
    {
        var user = new User { Id = 2, Username = "viewer", Email = "viewer@test.com" };
        var roleIds = new List<int> { 5 };

        _userRepoMock.Setup(r => r.GetUserByEmailAsync("viewer@test.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(2)).ReturnsAsync(roleIds);
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(roleIds)).ReturnsAsync(new List<Menu>());

        var dto = new GenerateTokenDto { Username = "viewer", Email = "viewer@test.com" };
        var result = await _service.GenerateAppTokenAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Empty(result.Menus);
    }

    [Fact]
    public async Task GenerateAppTokenAsync_NoRoleNamesFromMenus_FallsBackToRoleIds()
    {
        var user = new User { Id = 3, Username = "user3", Email = "u3@test.com" };
        var roleIds = new List<int> { 10 };
        var menus = new List<Menu>
        {
            new()
            {
                Id = 1, Name = "Settings", Route = "/settings", DisplayOrder = 1,
                RoleMenuPermissions = [new RoleMenuPermission { RoleId = 99, Role = new Role { Id = 99, Name = "Other" } }]
            }
        };

        _userRepoMock.Setup(r => r.GetUserByEmailAsync("u3@test.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(3)).ReturnsAsync(roleIds);
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(roleIds)).ReturnsAsync(menus);

        var dto = new GenerateTokenDto { Username = "user3", Email = "u3@test.com" };
        var result = await _service.GenerateAppTokenAsync(dto);

        Assert.Contains("role_10", result.Roles);
    }

    [Fact]
    public async Task GenerateAppTokenAsync_BuildsMenuHierarchy()
    {
        var user = new User { Id = 1, Username = "u", Email = "u@t.com" };
        var roleIds = new List<int> { 1 };
        var menus = new List<Menu>
        {
            new()
            {
                Id = 1, Name = "Parent", Route = "/parent", DisplayOrder = 1, ParentMenuId = null,
                RoleMenuPermissions = [new RoleMenuPermission { RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } }]
            },
            new()
            {
                Id = 2, Name = "Child", Route = "/parent/child", DisplayOrder = 2, ParentMenuId = 1,
                RoleMenuPermissions = [new RoleMenuPermission { RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } }]
            }
        };

        _userRepoMock.Setup(r => r.GetUserByEmailAsync("u@t.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(1)).ReturnsAsync(roleIds);
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(roleIds)).ReturnsAsync(menus);

        var dto = new GenerateTokenDto { Username = "u", Email = "u@t.com" };
        var result = await _service.GenerateAppTokenAsync(dto);

        Assert.Single(result.Menus);
        Assert.Equal("Parent", result.Menus[0].Name);
        Assert.Single(result.Menus[0].Children);
        Assert.Equal("Child", result.Menus[0].Children[0].Name);
    }

    [Fact]
    public async Task GenerateAppTokenAsync_NoJwtSecret_ThrowsInvalidOperation()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Secret"]).Returns((string?)null);

        var service = new AuthTokenService(_menuRepoMock.Object, _userRepoMock.Object, configMock.Object);

        var user = new User { Id = 1, Username = "u", Email = "u@t.com" };
        _userRepoMock.Setup(r => r.GetUserByEmailAsync("u@t.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(1)).ReturnsAsync(new List<int>());
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Menu>());

        var dto = new GenerateTokenDto { Username = "u", Email = "u@t.com" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateAppTokenAsync(dto));
    }

    [Fact]
    public async Task GenerateAppTokenAsync_MultipleRoles_AllIncluded()
    {
        var user = new User { Id = 1, Username = "multi", Email = "multi@t.com" };
        var roleIds = new List<int> { 1, 2 };
        var menus = new List<Menu>
        {
            new()
            {
                Id = 1, Name = "M1", Route = "/m1", DisplayOrder = 1,
                RoleMenuPermissions =
                [
                    new RoleMenuPermission { RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } },
                    new RoleMenuPermission { RoleId = 2, Role = new Role { Id = 2, Name = "Editor" } }
                ]
            }
        };

        _userRepoMock.Setup(r => r.GetUserByEmailAsync("multi@t.com")).ReturnsAsync(user);
        _menuRepoMock.Setup(r => r.GetUserRoleIdsAsync(1)).ReturnsAsync(roleIds);
        _menuRepoMock.Setup(r => r.GetMenusByRoleIdsAsync(roleIds)).ReturnsAsync(menus);

        var dto = new GenerateTokenDto { Username = "multi", Email = "multi@t.com" };
        var result = await _service.GenerateAppTokenAsync(dto);

        Assert.Contains("Admin", result.Roles);
        Assert.Contains("Editor", result.Roles);
    }
}
