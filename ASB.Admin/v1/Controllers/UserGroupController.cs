namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserGroupController : ControllerBase
    {
        private readonly IUserGroupService _userGroupService;

        public UserGroupController(IUserGroupService userGroupService)
        {
            _userGroupService = userGroupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _userGroupService.GetAllAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _userGroupService.GetByIdAsync(id);
            if (group is null)
                return NotFound();
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserGroupRequest request)
        {
            var dto = new ASB.Services.v1.Dtos.CreateUserGroupDto
            {
                GroupName = request.GroupName
            };

            var group = await _userGroupService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }

        [HttpPost("{groupId}/users/{userId}")]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            await _userGroupService.AddUserToGroupAsync(userId, groupId);
            return NoContent();
        }

        [HttpPost("{groupId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRoleToGroup(int groupId, int roleId)
        {
            await _userGroupService.AssignRoleToGroupAsync(groupId, roleId);
            return NoContent();
        }
    }
}
