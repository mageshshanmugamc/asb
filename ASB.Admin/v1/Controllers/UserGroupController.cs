namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [AsbAuthorize]
    public class UserGroupController : ControllerBase
    {
        private readonly IUserGroupService _userGroupService;

        public UserGroupController(IUserGroupService userGroupService)
        {
            _userGroupService = userGroupService;
        }

        [HttpGet]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _userGroupService.GetAllAsync();
            return Ok(UserGroupResponse.FromDtoList(groups));
        }

        [HttpGet("{id}")]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _userGroupService.GetByIdAsync(id);
            if (group is null)
                return NotFound();
            return Ok(UserGroupResponse.FromDto(group));
        }

        [HttpPost]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> Create([FromBody] CreateUserGroupRequest request)
        {
            var dto = new ASB.Services.v1.Dtos.CreateUserGroupDto
            {
                GroupName = request.GroupName,
                RoleIds = request.RoleIds
            };

            var group = await _userGroupService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, UserGroupResponse.FromDto(group));
        }

        [HttpPut("{id}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserGroupRequest request)
        {
            var dto = new ASB.Services.v1.Dtos.UpdateUserGroupDto
            {
                GroupName = request.GroupName,
                RoleIds = request.RoleIds
            };

            var group = await _userGroupService.UpdateAsync(id, dto);
            return Ok(UserGroupResponse.FromDto(group));
        }

        [HttpPost("{groupId}/users/{userId}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            await _userGroupService.AddUserToGroupAsync(userId, groupId);
            return NoContent();
        }

        [HttpPost("{groupId}/roles/{roleId}")]
        [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> AssignRoleToGroup(int groupId, int roleId)
        {
            await _userGroupService.AssignRoleToGroupAsync(groupId, roleId);
            return NoContent();
        }
    }
}
