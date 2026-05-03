namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

public class UserGroupServiceTests
{
    private readonly Mock<IUserGroupRepository> _userGroupRepoMock;
    private readonly UserGroupService _service;

    public UserGroupServiceTests()
    {
        _userGroupRepoMock = new Mock<IUserGroupRepository>();
        _service = new UserGroupService(_userGroupRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGroups()
    {
        var groups = new List<UserGroup>
        {
            new()
            {
                Id = 1, GroupName = "Admin",
                UserGroupMappings = [new UserGroupMapping { User = new User { Id = 1, Username = "u1", Email = "u1@t.com" } }],
                UserGroupRoles = [new UserGroupRole { Role = new Role { Id = 1, Name = "SuperAdmin" } }]
            },
            new() { Id = 2, GroupName = "Viewer", UserGroupMappings = [], UserGroupRoles = [] }
        };

        _userGroupRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(groups);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Admin", result[0].GroupName);
        Assert.Single(result[0].Users);
        Assert.Single(result[0].Roles);
        Assert.Empty(result[1].Users);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        _userGroupRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<UserGroup>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var group = new UserGroup
        {
            Id = 1, GroupName = "Editors",
            UserGroupMappings = [],
            UserGroupRoles = []
        };

        _userGroupRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Editors", result!.GroupName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _userGroupRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((UserGroup?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedGroup()
    {
        var dto = new CreateUserGroupDto { GroupName = "NewGroup" };

        _userGroupRepoMock.Setup(r => r.CreateAsync(It.Is<UserGroup>(g => g.GroupName == "NewGroup")))
            .ReturnsAsync(new UserGroup { Id = 5, GroupName = "NewGroup", UserGroupMappings = [], UserGroupRoles = [] });

        var result = await _service.CreateAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("NewGroup", result.GroupName);
    }

    [Fact]
    public async Task AddUserToGroupAsync_CallsRepository()
    {
        _userGroupRepoMock.Setup(r => r.AddUserToGroupAsync(1, 2)).Returns(Task.CompletedTask);

        await _service.AddUserToGroupAsync(1, 2);

        _userGroupRepoMock.Verify(r => r.AddUserToGroupAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToGroupAsync_NewAssignment_CallsRepository()
    {
        _userGroupRepoMock.Setup(r => r.RoleAssignmentExistsAsync(3, 7)).ReturnsAsync(false);
        _userGroupRepoMock.Setup(r => r.AssignRoleToGroupAsync(3, 7)).Returns(Task.CompletedTask);

        await _service.AssignRoleToGroupAsync(3, 7);

        _userGroupRepoMock.Verify(r => r.AssignRoleToGroupAsync(3, 7), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToGroupAsync_DuplicateAssignment_ThrowsInvalidOperation()
    {
        _userGroupRepoMock.Setup(r => r.RoleAssignmentExistsAsync(3, 7)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignRoleToGroupAsync(3, 7));
    }

    [Fact]
    public async Task GetByIdAsync_MapsUsersAndRoles()
    {
        var group = new UserGroup
        {
            Id = 1, GroupName = "Admin",
            UserGroupMappings = [new UserGroupMapping { User = new User { Id = 10, Username = "admin1", Email = "admin@t.com" } }],
            UserGroupRoles = [new UserGroupRole { Role = new Role { Id = 2, Name = "Manager" } }]
        };

        _userGroupRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Single(result!.Users);
        Assert.Equal("admin1", result.Users[0].Username);
        Assert.Single(result.Roles);
        Assert.Equal("Manager", result.Roles[0].Name);
    }

    [Fact]
    public async Task CreateAsync_WithRoleIds_PassesRolesToRepository()
    {
        var dto = new CreateUserGroupDto { GroupName = "DevTeam", RoleIds = [1, 3] };

        _userGroupRepoMock
            .Setup(r => r.CreateAsync(It.Is<UserGroup>(g =>
                g.GroupName == "DevTeam" && g.UserGroupRoles.Count == 2)))
            .ReturnsAsync(new UserGroup
            {
                Id = 10, GroupName = "DevTeam", UserGroupMappings = [],
                UserGroupRoles = [
                    new UserGroupRole { RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } },
                    new UserGroupRole { RoleId = 3, Role = new Role { Id = 3, Name = "Editor" } }
                ]
            });

        var result = await _service.CreateAsync(dto);

        Assert.Equal(2, result.Roles.Count);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsUpdatedGroup()
    {
        var dto = new UpdateUserGroupDto { GroupName = "Updated", RoleIds = [2] };

        _userGroupRepoMock
            .Setup(r => r.UpdateAsync(It.Is<UserGroup>(g =>
                g.Id == 5 && g.GroupName == "Updated" && g.UserGroupRoles.Count == 1)))
            .ReturnsAsync(new UserGroup
            {
                Id = 5, GroupName = "Updated", UserGroupMappings = [],
                UserGroupRoles = [new UserGroupRole { RoleId = 2, Role = new Role { Id = 2, Name = "Viewer" } }]
            });

        var result = await _service.UpdateAsync(5, dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("Updated", result.GroupName);
        Assert.Single(result.Roles);
        Assert.Equal("Viewer", result.Roles[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_EmptyRoleIds_ClearsRoles()
    {
        var dto = new UpdateUserGroupDto { GroupName = "NoRoles", RoleIds = [] };

        _userGroupRepoMock
            .Setup(r => r.UpdateAsync(It.Is<UserGroup>(g =>
                g.Id == 3 && g.UserGroupRoles.Count == 0)))
            .ReturnsAsync(new UserGroup { Id = 3, GroupName = "NoRoles", UserGroupMappings = [], UserGroupRoles = [] });

        var result = await _service.UpdateAsync(3, dto);

        Assert.Empty(result.Roles);
    }
}
