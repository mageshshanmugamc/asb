namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Admin.v1.Requests;
using ASB.Admin.v1.Response;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class UserGroupControllerTests
{
    private readonly Mock<IUserGroupService> _userGroupServiceMock;
    private readonly UserGroupController _controller;

    public UserGroupControllerTests()
    {
        _userGroupServiceMock = new Mock<IUserGroupService>();
        _controller = new UserGroupController(_userGroupServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithGroups()
    {
        var groups = new List<UserGroupDto>
        {
            new() { Id = 1, GroupName = "Admin", Users = [], Roles = [] },
            new() { Id = 2, GroupName = "Viewer", Users = [], Roles = [] }
        };

        _userGroupServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(groups);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroups = Assert.IsAssignableFrom<IEnumerable<UserGroupResponse>>(okResult.Value);
        Assert.Equal(2, returnedGroups.Count());
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
    {
        _userGroupServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<UserGroupDto>());

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroups = Assert.IsAssignableFrom<IEnumerable<UserGroupResponse>>(okResult.Value);
        Assert.Empty(returnedGroups);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var group = new UserGroupDto { Id = 1, GroupName = "Admin", Users = [], Roles = [] };

        _userGroupServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(group);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroup = Assert.IsType<UserGroupResponse>(okResult.Value);
        Assert.Equal("Admin", returnedGroup.GroupName);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _userGroupServiceMock
            .Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserGroupDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new CreateUserGroupRequest { GroupName = "Editors" };
        var createdGroup = new UserGroupDto { Id = 3, GroupName = "Editors", Users = [], Roles = [] };

        _userGroupServiceMock
            .Setup(s => s.CreateAsync(It.Is<CreateUserGroupDto>(d => d.GroupName == "Editors")))
            .ReturnsAsync(createdGroup);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedGroup = Assert.IsType<UserGroupResponse>(createdResult.Value);
        Assert.Equal("Editors", returnedGroup.GroupName);
    }

    [Fact]
    public async Task Create_ValidRequest_PassesCorrectDto()
    {
        var request = new CreateUserGroupRequest { GroupName = "NewGroup" };

        _userGroupServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserGroupDto>()))
            .ReturnsAsync(new UserGroupDto { Id = 5, GroupName = "NewGroup", Users = [], Roles = [] });

        await _controller.Create(request);

        _userGroupServiceMock.Verify(
            s => s.CreateAsync(It.Is<CreateUserGroupDto>(d => d.GroupName == "NewGroup")),
            Times.Once);
    }

    [Fact]
    public async Task AddUserToGroup_ValidIds_ReturnsNoContent()
    {
        _userGroupServiceMock
            .Setup(s => s.AddUserToGroupAsync(5, 3))
            .Returns(Task.CompletedTask);

        var result = await _controller.AddUserToGroup(3, 5);

        Assert.IsType<NoContentResult>(result);
        _userGroupServiceMock.Verify(s => s.AddUserToGroupAsync(5, 3), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToGroup_ValidIds_ReturnsNoContent()
    {
        _userGroupServiceMock
            .Setup(s => s.AssignRoleToGroupAsync(2, 10))
            .Returns(Task.CompletedTask);

        var result = await _controller.AssignRoleToGroup(2, 10);

        Assert.IsType<NoContentResult>(result);
        _userGroupServiceMock.Verify(s => s.AssignRoleToGroupAsync(2, 10), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsGroupWithUsersAndRoles()
    {
        var group = new UserGroupDto
        {
            Id = 1,
            GroupName = "Admin",
            Users = [new UserDto { Id = 1, Username = "user1", Email = "u1@test.com" }],
            Roles = [new RoleDto { Id = 1, Name = "SuperAdmin" }]
        };

        _userGroupServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(group);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroup = Assert.IsType<UserGroupResponse>(okResult.Value);
        Assert.Single(returnedGroup.Users);
        Assert.Single(returnedGroup.Roles);
    }

    [Fact]
    public async Task Create_WithRoleIds_PassesRoleIdsToService()
    {
        var request = new CreateUserGroupRequest { GroupName = "DevTeam", RoleIds = [1, 3] };

        _userGroupServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserGroupDto>()))
            .ReturnsAsync(new UserGroupDto { Id = 10, GroupName = "DevTeam", Users = [], Roles = [] });

        await _controller.Create(request);

        _userGroupServiceMock.Verify(
            s => s.CreateAsync(It.Is<CreateUserGroupDto>(d =>
                d.GroupName == "DevTeam" && d.RoleIds.Count == 2 && d.RoleIds.Contains(1) && d.RoleIds.Contains(3))),
            Times.Once);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var request = new UpdateUserGroupRequest { GroupName = "UpdatedGroup", RoleIds = [2, 4] };
        var updatedGroup = new UserGroupDto
        {
            Id = 1, GroupName = "UpdatedGroup", Users = [],
            Roles = [new RoleDto { Id = 2, Name = "Editor" }, new RoleDto { Id = 4, Name = "Viewer" }]
        };

        _userGroupServiceMock
            .Setup(s => s.UpdateAsync(1, It.IsAny<UpdateUserGroupDto>()))
            .ReturnsAsync(updatedGroup);

        var result = await _controller.Update(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroup = Assert.IsType<UserGroupResponse>(okResult.Value);
        Assert.Equal("UpdatedGroup", returnedGroup.GroupName);
        Assert.Equal(2, returnedGroup.Roles.Count);
    }

    [Fact]
    public async Task Update_PassesCorrectDtoToService()
    {
        var request = new UpdateUserGroupRequest { GroupName = "Renamed", RoleIds = [5] };

        _userGroupServiceMock
            .Setup(s => s.UpdateAsync(7, It.IsAny<UpdateUserGroupDto>()))
            .ReturnsAsync(new UserGroupDto { Id = 7, GroupName = "Renamed", Users = [], Roles = [] });

        await _controller.Update(7, request);

        _userGroupServiceMock.Verify(
            s => s.UpdateAsync(7, It.Is<UpdateUserGroupDto>(d =>
                d.GroupName == "Renamed" && d.RoleIds.Count == 1 && d.RoleIds.Contains(5))),
            Times.Once);
    }
}
