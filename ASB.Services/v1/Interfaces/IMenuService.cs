using ASB.Services.v1.Dtos;

namespace ASB.Services.v1.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuDto>> GetAllAsync();
        Task<MenuDto?> GetByIdAsync(int id);
        Task<MenuDto> CreateAsync(CreateMenuDto dto);
        Task<MenuDto> UpdateAsync(int id, UpdateMenuDto dto);
    }
}
