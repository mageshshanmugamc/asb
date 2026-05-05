namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [AsbAuthorize]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetAll()
        {
            var menus = await _menuService.GetAllAsync();
            return Ok(MenuResponse.FromDtoList(menus));
        }

        [HttpGet("{id}")]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            var menu = await _menuService.GetByIdAsync(id);
            if (menu is null)
                return NotFound();
            return Ok(MenuResponse.FromDto(menu));
        }

        [HttpPost]
        [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
        {
            var dto = new CreateMenuDto
            {
                Name = request.Name,
                Route = request.Route,
                Icon = request.Icon,
                DisplayOrder = request.DisplayOrder,
                ParentMenuId = request.ParentMenuId
            };

            var menu = await _menuService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = menu.Id }, MenuResponse.FromDto(menu));
        }

        [HttpPut("{id}")]
        [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuRequest request)
        {
            var dto = new UpdateMenuDto
            {
                Name = request.Name,
                Route = request.Route,
                Icon = request.Icon,
                DisplayOrder = request.DisplayOrder,
                ParentMenuId = request.ParentMenuId
            };

            var menu = await _menuService.UpdateAsync(id, dto);
            return Ok(MenuResponse.FromDto(menu));
        }
    }
}
