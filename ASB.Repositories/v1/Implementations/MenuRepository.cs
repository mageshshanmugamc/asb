using Microsoft.EntityFrameworkCore;
using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;

namespace ASB.Repositories.v1.Implementations;

public class MenuRepository : IMenuRepository
{
    private readonly AsbContext _context;

    public MenuRepository(AsbContext context)
    {
        _context = context;
    }

    public async Task<List<Menu>> GetMenusByRoleIdsAsync(IEnumerable<int> roleIds)
    {
        var menuIds = await _context.RoleMenuPermissions
            .Where(rmp => roleIds.Contains(rmp.RoleId))
            .Select(rmp => rmp.MenuId)
            .Distinct()
            .ToListAsync();

        return await _context.Menus
            .Include(m => m.RoleMenuPermissions)
                .ThenInclude(rmp => rmp.Role)
            .Where(m => menuIds.Contains(m.Id))
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserRoleIdsAsync(int userId)
    {
        // Roles via user groups only
        return await _context.UserGroupMappings
            .Where(ugm => ugm.UserId == userId)
            .SelectMany(ugm => ugm.UserGroup.UserGroupRoles)
            .Select(ugr => ugr.RoleId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<string>> GetPolicyNamesByRoleIdsAsync(IEnumerable<int> roleIds)
    {
        return await _context.RolePolicies
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Policy.Name)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<Menu>> GetAllAsync()
    {
        return await _context.Menus
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Menu?> GetByIdAsync(int id)
    {
        return await _context.Menus.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Menu> CreateAsync(Menu menu)
    {
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();
        return menu;
    }

    public async Task<Menu> UpdateAsync(Menu menu)
    {
        var existing = await _context.Menus
            .FirstOrDefaultAsync(m => m.Id == menu.Id)
            ?? throw new KeyNotFoundException($"Menu with Id {menu.Id} not found.");

        existing.Name = menu.Name;
        existing.Route = menu.Route;
        existing.Icon = menu.Icon;
        existing.DisplayOrder = menu.DisplayOrder;
        existing.ParentMenuId = menu.ParentMenuId;

        await _context.SaveChangesAsync();
        return existing;
    }
}
