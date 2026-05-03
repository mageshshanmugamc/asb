namespace ASB.Admin.Tests.Repositories;

using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UserGroupRepositoryTests : IDisposable
{
    private readonly AsbContext _context;
    private readonly UserGroupRepository _repository;

    public UserGroupRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AsbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AsbContext(options);
        _repository = new UserGroupRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsGroupsWithIncludedData()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _context.Roles.Add(role);

        var group = new UserGroup { Id = 1, GroupName = "Admins" };
        _context.UserGroups.Add(group);

        var user = new User { Id = 1, Username = "u1", Email = "u1@t.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserGroupMappings.Add(new UserGroupMapping { UserId = 1, UserGroupId = 1 });
        _context.UserGroupRoles.Add(new UserGroupRole { UserGroupId = 1, RoleId = 1 });
        await _context.SaveChangesAsync();

        var result = (await _repository.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Admins", result[0].GroupName);
        Assert.Single(result[0].UserGroupMappings);
        Assert.Single(result[0].UserGroupRoles);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmpty()
    {
        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsGroupWithIncludes()
    {
        var group = new UserGroup { Id = 2, GroupName = "Viewers" };
        _context.UserGroups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(2);

        Assert.NotNull(result);
        Assert.Equal("Viewers", result!.GroupName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsGroupAndReturnsIt()
    {
        var group = new UserGroup { GroupName = "NewGroup" };

        var result = await _repository.CreateAsync(group);

        Assert.True(result.Id > 0);
        Assert.Equal("NewGroup", result.GroupName);
        Assert.Single(_context.UserGroups);
    }

    [Fact]
    public async Task AddUserToGroupAsync_ValidIds_CreatesMapping()
    {
        _context.UserGroups.Add(new UserGroup { Id = 3, GroupName = "G3" });
        _context.Users.Add(new User { Id = 5, Username = "u5", Email = "u5@t.com" });
        await _context.SaveChangesAsync();

        await _repository.AddUserToGroupAsync(5, 3);

        var mapping = await _context.UserGroupMappings.FirstOrDefaultAsync(m => m.UserId == 5 && m.UserGroupId == 3);
        Assert.NotNull(mapping);
    }

    [Fact]
    public async Task AddUserToGroupAsync_GroupNotFound_ThrowsKeyNotFound()
    {
        _context.Users.Add(new User { Id = 6, Username = "u6", Email = "u6@t.com" });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.AddUserToGroupAsync(6, 999));
    }

    [Fact]
    public async Task AddUserToGroupAsync_UserNotFound_ThrowsKeyNotFound()
    {
        _context.UserGroups.Add(new UserGroup { Id = 4, GroupName = "G4" });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.AddUserToGroupAsync(999, 4));
    }

    [Fact]
    public async Task AssignRoleToGroupAsync_CreatesRoleAssignment()
    {
        _context.UserGroups.Add(new UserGroup { Id = 5, GroupName = "G5" });
        _context.Roles.Add(new Role { Id = 2, Name = "Editor" });
        await _context.SaveChangesAsync();

        await _repository.AssignRoleToGroupAsync(5, 2);

        var assignment = await _context.UserGroupRoles.FirstOrDefaultAsync(r => r.UserGroupId == 5 && r.RoleId == 2);
        Assert.NotNull(assignment);
    }

    [Fact]
    public async Task RoleAssignmentExistsAsync_Exists_ReturnsTrue()
    {
        _context.UserGroups.Add(new UserGroup { Id = 6, GroupName = "G6" });
        _context.Roles.Add(new Role { Id = 3, Name = "Manager" });
        _context.UserGroupRoles.Add(new UserGroupRole { UserGroupId = 6, RoleId = 3 });
        await _context.SaveChangesAsync();

        var result = await _repository.RoleAssignmentExistsAsync(6, 3);

        Assert.True(result);
    }

    [Fact]
    public async Task RoleAssignmentExistsAsync_NotExists_ReturnsFalse()
    {
        var result = await _repository.RoleAssignmentExistsAsync(99, 99);

        Assert.False(result);
    }
}
