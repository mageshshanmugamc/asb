using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;

namespace ASB.Services.v1.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<IEnumerable<MenuDto>> GetAllAsync()
        {
            var menus = await _menuRepository.GetAllAsync();
            return menus.Select(MapToDto);
        }

        public async Task<MenuDto?> GetByIdAsync(int id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            return menu is null ? null : MapToDto(menu);
        }

        public async Task<MenuDto> CreateAsync(CreateMenuDto dto)
        {
            var menu = new Menu
            {
                Name = dto.Name,
                Route = dto.Route,
                Icon = dto.Icon,
                DisplayOrder = dto.DisplayOrder,
                ParentMenuId = dto.ParentMenuId
            };
            var created = await _menuRepository.CreateAsync(menu);
            return MapToDto(created);
        }

        public async Task<MenuDto> UpdateAsync(int id, UpdateMenuDto dto)
        {
            var menu = new Menu
            {
                Id = id,
                Name = dto.Name,
                Route = dto.Route,
                Icon = dto.Icon,
                DisplayOrder = dto.DisplayOrder,
                ParentMenuId = dto.ParentMenuId
            };
            var updated = await _menuRepository.UpdateAsync(menu);
            return MapToDto(updated);
        }

        private static MenuDto MapToDto(Menu menu) => new()
        {
            Id = menu.Id,
            Name = menu.Name,
            Route = menu.Route,
            Icon = menu.Icon,
            DisplayOrder = menu.DisplayOrder
        };
    }
}
