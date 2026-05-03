namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _service = new RoleService(_roleRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRoles()
    {
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Admin" },
            new() { Id = 2, Name = "Viewer" }
        };

        _roleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Admin", result[0].Name);
        Assert.Equal("Viewer", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        _roleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Role>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var role = new Role { Id = 1, Name = "Editor" };

        _roleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(role);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Editor", result!.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Role?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedRole()
    {
        var dto = new CreateRoleDto { Name = "NewRole" };

        _roleRepoMock.Setup(r => r.CreateAsync(It.Is<Role>(role => role.Name == "NewRole")))
            .ReturnsAsync(new Role { Id = 5, Name = "NewRole" });

        var result = await _service.CreateAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("NewRole", result.Name);
    }

    [Fact]
    public async Task AssignPolicyToRoleAsync_NewAssignment_CallsRepository()
    {
        _roleRepoMock.Setup(r => r.PolicyAssignmentExistsAsync(3, 7)).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.AssignPolicyToRoleAsync(3, 7)).Returns(Task.CompletedTask);

        await _service.AssignPolicyToRoleAsync(3, 7);

        _roleRepoMock.Verify(r => r.AssignPolicyToRoleAsync(3, 7), Times.Once);
    }

    [Fact]
    public async Task AssignPolicyToRoleAsync_DuplicateAssignment_ThrowsInvalidOperation()
    {
        _roleRepoMock.Setup(r => r.PolicyAssignmentExistsAsync(3, 7)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignPolicyToRoleAsync(3, 7));
    }

    [Fact]
    public async Task AssignPolicyToRoleAsync_DuplicateAssignment_DoesNotCallAssign()
    {
        _roleRepoMock.Setup(r => r.PolicyAssignmentExistsAsync(3, 7)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignPolicyToRoleAsync(3, 7));

        _roleRepoMock.Verify(r => r.AssignPolicyToRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
