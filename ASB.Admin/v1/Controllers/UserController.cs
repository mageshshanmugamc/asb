namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Infrastructure;
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
        private readonly IKeycloakAdminService _keycloakAdminService;

        public UserController(IUserService userService, IKeycloakAdminService keycloakAdminService)
        {
            this.userService = userService;
            _keycloakAdminService = keycloakAdminService;
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
                UserGroupId = request.UserGroupId ?? 4
            };

            var user = await userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Username, user.Email });
        }

        [HttpPost("{userId}/groups/{groupId}")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> AddUserToGroup(int userId, int groupId)
        {
            await userService.AddUserToGroupAsync(userId, groupId);
            return NoContent();
        }

        [HttpGet("keycloak/lookup")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> LookupKeycloakUser([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { error = "email is required." });

            var kcUser = await _keycloakAdminService.GetUserByEmailAsync(email);
            if (kcUser == null)
                return NotFound(new { error = "User not found in Keycloak." });

            return Ok(new
            {
                username = kcUser.Username,
                email = kcUser.Email,
                firstName = kcUser.FirstName,
                lastName = kcUser.LastName,
                fullName = $"{kcUser.FirstName} {kcUser.LastName}".Trim()
            });
        }
    }
}