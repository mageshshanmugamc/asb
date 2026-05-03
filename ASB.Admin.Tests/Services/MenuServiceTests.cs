namespace ASB.Admin.Tests.Services;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Implementations;
using Moq;
using Xunit;

public class MenuServiceTests
{
    private readonly Mock<IMenuRepository> _menuRepoMock;
    private readonly MenuService _service;

    public MenuServiceTests()
    {
        _menuRepoMock = new Mock<IMenuRepository>();
        _service = new MenuService(_menuRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMenus()
    {
        var menus = new List<Menu>
        {
            new() { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 1 },
            new() { Id = 2, Name = "Settings", Route = "/settings", Icon = "settings", DisplayOrder = 2 }
        };

        _menuRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(menus);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Dashboard", result[0].Name);
        Assert.Equal("Settings", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        _menuRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Menu>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var menu = new Menu { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 1 };

        _menuRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(menu);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Dashboard", result!.Name);
        Assert.Equal("/dashboard", result.Route);
        Assert.Equal("dashboard", result.Icon);
        Assert.Equal(1, result.DisplayOrder);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _menuRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Menu?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedMenu()
    {
        var dto = new CreateMenuDto { Name = "Reports", Route = "/reports", Icon = "report", DisplayOrder = 3 };

        _menuRepoMock
            .Setup(r => r.CreateAsync(It.Is<Menu>(m => m.Name == "Reports" && m.Route == "/reports")))
            .ReturnsAsync(new Menu { Id = 5, Name = "Reports", Route = "/reports", Icon = "report", DisplayOrder = 3 });

        var result = await _service.CreateAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("Reports", result.Name);
        Assert.Equal("/reports", result.Route);
    }

    [Fact]
    public async Task CreateAsync_MapsAllFieldsToEntity()
    {
        var dto = new CreateMenuDto { Name = "N", Route = "/n", Icon = "ic", DisplayOrder = 4, ParentMenuId = 1 };

        _menuRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Menu>()))
            .ReturnsAsync(new Menu { Id = 1, Name = "N", Route = "/n", Icon = "ic", DisplayOrder = 4, ParentMenuId = 1 });

        await _service.CreateAsync(dto);

        _menuRepoMock.Verify(r => r.CreateAsync(It.Is<Menu>(m =>
            m.Name == "N" && m.Route == "/n" && m.Icon == "ic" && m.DisplayOrder == 4 && m.ParentMenuId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsUpdatedMenu()
    {
        var dto = new UpdateMenuDto { Name = "Updated", Route = "/updated", Icon = "edit", DisplayOrder = 5 };

        _menuRepoMock
            .Setup(r => r.UpdateAsync(It.Is<Menu>(m => m.Id == 3 && m.Name == "Updated")))
            .ReturnsAsync(new Menu { Id = 3, Name = "Updated", Route = "/updated", Icon = "edit", DisplayOrder = 5 });

        var result = await _service.UpdateAsync(3, dto);

        Assert.Equal(3, result.Id);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("/updated", result.Route);
        Assert.Equal("edit", result.Icon);
        Assert.Equal(5, result.DisplayOrder);
    }

    [Fact]
    public async Task UpdateAsync_MapsAllFieldsToEntity()
    {
        var dto = new UpdateMenuDto { Name = "U", Route = "/u", Icon = "ic", DisplayOrder = 2, ParentMenuId = 5 };

        _menuRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Menu>()))
            .ReturnsAsync(new Menu { Id = 7, Name = "U", Route = "/u", Icon = "ic", DisplayOrder = 2, ParentMenuId = 5 });

        await _service.UpdateAsync(7, dto);

        _menuRepoMock.Verify(r => r.UpdateAsync(It.Is<Menu>(m =>
            m.Id == 7 && m.Name == "U" && m.Route == "/u" && m.Icon == "ic" && m.DisplayOrder == 2 && m.ParentMenuId == 5)),
            Times.Once);
    }
}
