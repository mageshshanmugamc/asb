using ASB.Repositories.v1.Entities;

namespace ASB.Repositories.v1.Interfaces;

public interface IMenuRepository
{
    Task<List<Menu>> GetMenusByRoleIdsAsync(IEnumerable<int> roleIds);
    Task<List<int>> GetUserRoleIdsAsync(int userId);
    Task<List<string>> GetPolicyNamesByRoleIdsAsync(IEnumerable<int> roleIds);
    Task<IEnumerable<Menu>> GetAllAsync();
    Task<Menu?> GetByIdAsync(int id);
    Task<Menu> CreateAsync(Menu menu);
    Task<Menu> UpdateAsync(Menu menu);
}
