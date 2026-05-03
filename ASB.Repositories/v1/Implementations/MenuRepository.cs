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
}
