namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet]
        [Authorize(Policy = Policies.ReadOnly)]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _policyService.GetAllAsync();
            return Ok(policies);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Policies.ReadOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy is null)
                return NotFound();
            return Ok(policy);
        }

        [HttpPost]
        [Authorize(Policy = Policies.FullAccess)]
        public async Task<IActionResult> Create([FromBody] CreatePolicyRequest request)
        {
            var dto = new CreatePolicyDto
            {
                Name = request.Name,
                Description = request.Description,
                Resource = request.Resource,
                Action = request.Action
            };

            var policy = await _policyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.FullAccess)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePolicyRequest request)
        {
            var dto = new UpdatePolicyDto
            {
                Name = request.Name,
                Description = request.Description,
                Resource = request.Resource,
                Action = request.Action
            };

            var policy = await _policyService.UpdateAsync(id, dto);
            return Ok(policy);
        }
    }
}
