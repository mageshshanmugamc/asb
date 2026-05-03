namespace ASB.Admin.Tests.Repositories;

using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UserRepositoryTests : IDisposable
{
    private readonly AsbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AsbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AsbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        _context.Users.Add(new User { Id = 1, Username = "user1", Email = "u1@test.com" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("user1", result!.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserNotFound_ReturnsNull()
    {
        var result = await _repository.GetUserByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUser()
    {
        _context.Users.Add(new User { Id = 2, Username = "user2", Email = "user2@test.com" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByEmailAsync("user2@test.com");

        Assert.NotNull(result);
        Assert.Equal("user2", result!.Username);
    }

    [Fact]
    public async Task GetUserByEmailAsync_NotFound_ReturnsNull()
    {
        var result = await _repository.GetUserByEmailAsync("nonexistent@test.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllWithMappings()
    {
        var group = new UserGroup { Id = 1, GroupName = "TestGroup" };
        _context.UserGroups.Add(group);

        var user = new User { Id = 3, Username = "user3", Email = "u3@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserGroupMappings.Add(new UserGroupMapping { UserId = 3, UserGroupId = 1 });
        await _context.SaveChangesAsync();

        var result = (await _repository.GetAllUsersAsync()).ToList();

        Assert.Single(result);
        Assert.Single(result[0].UserGroupMappings);
    }

    [Fact]
    public async Task GetAllUsersAsync_Empty_ReturnsEmpty()
    {
        var result = await _repository.GetAllUsersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateUserAsync_AddsUserAndReturnsIt()
    {
        var user = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "hash" };

        var result = await _repository.CreateUserAsync(user);

        Assert.True(result.Id > 0);
        Assert.Equal("newuser", result.Username);
        Assert.Single(_context.Users);
    }

    [Fact]
    public async Task AddUserToGroupAsync_ValidIds_CreatesMapping()
    {
        _context.Users.Add(new User { Id = 10, Username = "u", Email = "u@t.com" });
        _context.UserGroups.Add(new UserGroup { Id = 5, GroupName = "G" });
        await _context.SaveChangesAsync();

        await _repository.AddUserToGroupAsync(10, 5, "admin");

        var mapping = await _context.UserGroupMappings.FirstOrDefaultAsync(m => m.UserId == 10 && m.UserGroupId == 5);
        Assert.NotNull(mapping);
        Assert.Equal("admin", mapping!.AssignedBy);
    }

    [Fact]
    public async Task AddUserToGroupAsync_DuplicateMapping_ThrowsInvalidOperation()
    {
        _context.Users.Add(new User { Id = 11, Username = "u", Email = "u2@t.com" });
        _context.UserGroups.Add(new UserGroup { Id = 6, GroupName = "G2" });
        _context.UserGroupMappings.Add(new UserGroupMapping { UserId = 11, UserGroupId = 6 });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddUserToGroupAsync(11, 6));
    }

    [Fact]
    public async Task AddUserToGroupAsync_NullAssignedBy_SetsNull()
    {
        _context.Users.Add(new User { Id = 12, Username = "u12", Email = "u12@t.com" });
        _context.UserGroups.Add(new UserGroup { Id = 7, GroupName = "G7" });
        await _context.SaveChangesAsync();

        await _repository.AddUserToGroupAsync(12, 7);

        var mapping = await _context.UserGroupMappings.FirstAsync(m => m.UserId == 12);
        Assert.Null(mapping.AssignedBy);
    }
}
