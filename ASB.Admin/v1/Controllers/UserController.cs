namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = Policies.ReadOnly)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userService.GetUsers();
            return Ok(UserResponse.DtoListToUsersList(users));
        }

        [HttpPost]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var dto = new CreateUserDto
            {
                Username = request.Username,
                Email = request.Email,
                UserGroupIds = request.UserGroupIds
            };

            var user = await userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Username, user.Email, user.UserGroupIds });
        }

        [HttpPost("{userId}/groups/{groupId}")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> AddUserToGroup(int userId, int groupId)
        {
            await userService.AddUserToGroupAsync(userId, groupId);
            return NoContent();
        }
    }
}