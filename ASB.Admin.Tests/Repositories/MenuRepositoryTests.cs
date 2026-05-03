namespace ASB.Admin.Tests.Repositories;

using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MenuRepositoryTests : IDisposable
{
    private readonly AsbContext _context;
    private readonly MenuRepository _repository;

    public MenuRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AsbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AsbContext(options);
        _repository = new MenuRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_ReturnsMatchingMenus()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _context.Roles.Add(role);

        var menu1 = new Menu { Id = 1, Name = "Dashboard", Route = "/dashboard", DisplayOrder = 1 };
        var menu2 = new Menu { Id = 2, Name = "Settings", Route = "/settings", DisplayOrder = 2 };
        _context.Menus.AddRange(menu1, menu2);

        _context.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = 1, MenuId = 1 });
        _context.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = 1, MenuId = 2 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 1 });

        Assert.Equal(2, result.Count);
        Assert.Equal("Dashboard", result[0].Name);
        Assert.Equal("Settings", result[1].Name);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 999 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_MultipleRoles_ReturnsDistinctMenus()
    {
        _context.Roles.AddRange(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Editor" }
        );

        var menu = new Menu { Id = 1, Name = "SharedMenu", Route = "/shared", DisplayOrder = 1 };
        _context.Menus.Add(menu);

        _context.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = 1, MenuId = 1 });
        _context.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = 2, MenuId = 1 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 1, 2 });

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_IncludesRoleMenuPermissions()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _context.Roles.Add(role);

        var menu = new Menu { Id = 1, Name = "Dashboard", Route = "/dashboard", DisplayOrder = 1 };
        _context.Menus.Add(menu);

        _context.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = 1, MenuId = 1, PermissionLevel = "FullControl" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 1 });

        Assert.Single(result);
        Assert.Single(result[0].RoleMenuPermissions);
        Assert.Equal("Admin", result[0].RoleMenuPermissions.First().Role.Name);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_OrderedByDisplayOrder()
    {
        var role = new Role { Id = 1, Name = "Admin" };
        _context.Roles.Add(role);

        _context.Menus.AddRange(
            new Menu { Id = 1, Name = "Third", Route = "/third", DisplayOrder = 3 },
            new Menu { Id = 2, Name = "First", Route = "/first", DisplayOrder = 1 },
            new Menu { Id = 3, Name = "Second", Route = "/second", DisplayOrder = 2 }
        );

        _context.RoleMenuPermissions.AddRange(
            new RoleMenuPermission { RoleId = 1, MenuId = 1 },
            new RoleMenuPermission { RoleId = 1, MenuId = 2 },
            new RoleMenuPermission { RoleId = 1, MenuId = 3 }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 1 });

        Assert.Equal("First", result[0].Name);
        Assert.Equal("Second", result[1].Name);
        Assert.Equal("Third", result[2].Name);
    }

    [Fact]
    public async Task GetUserRoleIdsAsync_ReturnsRoleIdsViaGroups()
    {
        var user = new User { Id = 1, Username = "u1", Email = "u1@t.com" };
        _context.Users.Add(user);

        var group = new UserGroup { Id = 1, GroupName = "Admin" };
        _context.UserGroups.Add(group);

        _context.Roles.Add(new Role { Id = 1, Name = "Admin" });
        _context.UserGroupMappings.Add(new UserGroupMapping { UserId = 1, UserGroupId = 1 });
        _context.UserGroupRoles.Add(new UserGroupRole { UserGroupId = 1, RoleId = 1 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserRoleIdsAsync(1);

        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public async Task GetUserRoleIdsAsync_UserInMultipleGroups_ReturnsDistinctRoleIds()
    {
        var user = new User { Id = 2, Username = "u2", Email = "u2@t.com" };
        _context.Users.Add(user);

        _context.UserGroups.AddRange(
            new UserGroup { Id = 2, GroupName = "G1" },
            new UserGroup { Id = 3, GroupName = "G2" }
        );

        _context.Roles.AddRange(
            new Role { Id = 2, Name = "R1" },
            new Role { Id = 3, Name = "R2" }
        );

        _context.UserGroupMappings.AddRange(
            new UserGroupMapping { UserId = 2, UserGroupId = 2 },
            new UserGroupMapping { UserId = 2, UserGroupId = 3 }
        );

        _context.UserGroupRoles.AddRange(
            new UserGroupRole { UserGroupId = 2, RoleId = 2 },
            new UserGroupRole { UserGroupId = 3, RoleId = 3 }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserRoleIdsAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public async Task GetUserRoleIdsAsync_UserNoGroups_ReturnsEmpty()
    {
        var user = new User { Id = 3, Username = "lonely", Email = "lonely@t.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserRoleIdsAsync(3);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPolicyNamesByRoleIdsAsync_ReturnsPolicyNames()
    {
        _context.Roles.Add(new Role { Id = 1, Name = "Admin" });
        _context.Policies.Add(new Policy { Id = 1, Name = "FullAccess", Description = "Full access", Resource = "*", Action = "*" });
        _context.Policies.Add(new Policy { Id = 2, Name = "ReadOnly", Description = "Read only", Resource = "*", Action = "Read" });
        _context.RolePolicies.Add(new RolePolicy { RoleId = 1, PolicyId = 1 });
        _context.RolePolicies.Add(new RolePolicy { RoleId = 1, PolicyId = 2 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetPolicyNamesByRoleIdsAsync(new[] { 1 });

        Assert.Equal(2, result.Count);
        Assert.Contains("FullAccess", result);
        Assert.Contains("ReadOnly", result);
    }

    [Fact]
    public async Task GetPolicyNamesByRoleIdsAsync_NoRolePolicies_ReturnsEmpty()
    {
        var result = await _repository.GetPolicyNamesByRoleIdsAsync(new[] { 999 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPolicyNamesByRoleIdsAsync_MultipleRoles_ReturnsDistinct()
    {
        _context.Roles.AddRange(
            new Role { Id = 1, Name = "R1" },
            new Role { Id = 2, Name = "R2" }
        );
        _context.Policies.Add(new Policy { Id = 1, Name = "ReadOnly", Description = "Read", Resource = "*", Action = "Read" });
        _context.RolePolicies.AddRange(
            new RolePolicy { RoleId = 1, PolicyId = 1 },
            new RolePolicy { RoleId = 2, PolicyId = 1 }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetPolicyNamesByRoleIdsAsync(new[] { 1, 2 });

        Assert.Single(result);
        Assert.Contains("ReadOnly", result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMenusOrderedByDisplayOrder()
    {
        _context.Menus.AddRange(
            new Menu { Id = 1, Name = "Third", Route = "/third", DisplayOrder = 3 },
            new Menu { Id = 2, Name = "First", Route = "/first", DisplayOrder = 1 },
            new Menu { Id = 3, Name = "Second", Route = "/second", DisplayOrder = 2 }
        );
        await _context.SaveChangesAsync();

        var result = (await _repository.GetAllAsync()).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("First", result[0].Name);
        Assert.Equal("Second", result[1].Name);
        Assert.Equal("Third", result[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMenu()
    {
        _context.Menus.Add(new Menu { Id = 1, Name = "Dashboard", Route = "/dashboard", DisplayOrder = 1 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Dashboard", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsMenuAndReturnsIt()
    {
        var menu = new Menu { Name = "Reports", Route = "/reports", Icon = "report", DisplayOrder = 3 };

        var result = await _repository.CreateAsync(menu);

        Assert.True(result.Id > 0);
        Assert.Equal("Reports", result.Name);
        Assert.Equal("/reports", result.Route);
        Assert.Equal("report", result.Icon);
        Assert.Equal(3, result.DisplayOrder);

        var inDb = await _context.Menus.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Reports", inDb!.Name);
    }

    [Fact]
    public async Task UpdateAsync_Found_UpdatesAndReturnsMenu()
    {
        _context.Menus.Add(new Menu { Id = 1, Name = "Old", Route = "/old", Icon = "old_icon", DisplayOrder = 1 });
        await _context.SaveChangesAsync();

        var updated = new Menu { Id = 1, Name = "New", Route = "/new", Icon = "new_icon", DisplayOrder = 5, ParentMenuId = null };
        var result = await _repository.UpdateAsync(updated);

        Assert.Equal("New", result.Name);
        Assert.Equal("/new", result.Route);
        Assert.Equal("new_icon", result.Icon);
        Assert.Equal(5, result.DisplayOrder);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        var menu = new Menu { Id = 999, Name = "X", Route = "/x", DisplayOrder = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(menu));
    }
}
