namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Extensions;
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;

    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [AsbAuthorize]
    [EnableRateLimiting(RateLimitingExtensions.FixedPolicy)]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService userService = userService;

        [HttpGet]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetUsers([FromQuery] PaginatedQueryParams query)
        {
            var result = await userService.GetUsersAsync(query.ToPaginationQuery());
            return Ok(new PagedResponse<UserResponse>
            {
                Items = result.Items.Select(UserResponse.DtoToUsers),
                TotalCount = result.TotalCount,
                Skip = query.Skip,
                Take = query.Take
            });
        }

        [HttpPost]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var dto = new CreateUserDto
            {
                Username = request.Username,
                Email = request.Email,
                UserGroupIds = request.UserGroupIds
            };

            var user = await userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, UserResponse.DtoToUsers(user));
        }

        [HttpPost("{userId}/groups/{groupId}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> AddUserToGroup(int userId, int groupId)
        {
            await userService.AddUserToGroupAsync(userId, groupId);
            return NoContent();
        }
        [HttpGet("{id}")]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user is null)
                return NotFound();
            return Ok(UserResponse.DtoToUsers(user));
        }

        [HttpDelete("{id}")]
        [AsbAuthorize(Policies.ManageUsers)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}