namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Admin.v1.Response;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class MenuControllerTests
{
    private readonly Mock<IMenuService> _menuServiceMock;
    private readonly MenuController _controller;

    public MenuControllerTests()
    {
        _menuServiceMock = new Mock<IMenuService>();
        _controller = new MenuController(_menuServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithMenus()
    {
        var menus = new List<MenuDto>
        {
            new() { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 1 },
            new() { Id = 2, Name = "Settings", Route = "/settings", Icon = "settings", DisplayOrder = 2 }
        };

        _menuServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(menus);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<MenuResponse>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
    {
        _menuServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MenuDto>());

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<MenuResponse>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var menu = new MenuDto { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 1 };

        _menuServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(menu);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<MenuResponse>(okResult.Value);
        Assert.Equal("Dashboard", returned.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _menuServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((MenuDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new CreateMenuRequest { Name = "Reports", Route = "/reports", Icon = "report", DisplayOrder = 3 };
        var created = new MenuDto { Id = 5, Name = "Reports", Route = "/reports", Icon = "report", DisplayOrder = 3 };

        _menuServiceMock
            .Setup(s => s.CreateAsync(It.Is<CreateMenuDto>(d => d.Name == "Reports")))
            .ReturnsAsync(created);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        var returned = Assert.IsType<MenuResponse>(createdResult.Value);
        Assert.Equal("Reports", returned.Name);
    }

    [Fact]
    public async Task Create_PassesCorrectDto()
    {
        var request = new CreateMenuRequest { Name = "Users", Route = "/users", Icon = "people", DisplayOrder = 2, ParentMenuId = 1 };

        _menuServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateMenuDto>()))
            .ReturnsAsync(new MenuDto { Id = 10, Name = "Users", Route = "/users", Icon = "people", DisplayOrder = 2 });

        await _controller.Create(request);

        _menuServiceMock.Verify(
            s => s.CreateAsync(It.Is<CreateMenuDto>(d =>
                d.Name == "Users" && d.Route == "/users" && d.Icon == "people" && d.DisplayOrder == 2 && d.ParentMenuId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var request = new UpdateMenuRequest { Name = "Updated", Route = "/updated", Icon = "edit", DisplayOrder = 5 };
        var updated = new MenuDto { Id = 1, Name = "Updated", Route = "/updated", Icon = "edit", DisplayOrder = 5 };

        _menuServiceMock
            .Setup(s => s.UpdateAsync(1, It.IsAny<UpdateMenuDto>()))
            .ReturnsAsync(updated);

        var result = await _controller.Update(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<MenuResponse>(okResult.Value);
        Assert.Equal("Updated", returned.Name);
    }

    [Fact]
    public async Task Update_PassesCorrectIdAndDto()
    {
        var request = new UpdateMenuRequest { Name = "Renamed", Route = "/renamed", Icon = "icon", DisplayOrder = 7, ParentMenuId = 2 };

        _menuServiceMock
            .Setup(s => s.UpdateAsync(7, It.IsAny<UpdateMenuDto>()))
            .ReturnsAsync(new MenuDto { Id = 7, Name = "Renamed", Route = "/renamed", Icon = "icon", DisplayOrder = 7 });

        await _controller.Update(7, request);

        _menuServiceMock.Verify(
            s => s.UpdateAsync(7, It.Is<UpdateMenuDto>(d =>
                d.Name == "Renamed" && d.Route == "/renamed" && d.ParentMenuId == 2)),
            Times.Once);
    }
}
