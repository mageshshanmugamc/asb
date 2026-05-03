namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Authorization;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Policy = Policies.ReadOnly)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Policies.ReadOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role is null)
                return NotFound();
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = Policies.FullAccess)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            var dto = new ASB.Services.v1.Dtos.CreateRoleDto
            {
                Name = request.Name
            };

            var role = await _roleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPost("{roleId}/policies/{policyId}")]
        [Authorize(Policy = Policies.FullAccess)]
        public async Task<IActionResult> AssignPolicyToRole(int roleId, int policyId)
        {
            await _roleService.AssignPolicyToRoleAsync(roleId, policyId);
            return NoContent();
        }
    }
}
