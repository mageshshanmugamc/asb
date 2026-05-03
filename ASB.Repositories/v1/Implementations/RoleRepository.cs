using Microsoft.EntityFrameworkCore;
using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;

namespace ASB.Repositories.v1.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AsbContext _context;

    public RoleRepository(AsbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.RolePolicies)
                .ThenInclude(rp => rp.Policy)
            .Include(r => r.UserGroupRoles)
                .ThenInclude(ugr => ugr.UserGroup)
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.RolePolicies)
                .ThenInclude(rp => rp.Policy)
            .Include(r => r.UserGroupRoles)
                .ThenInclude(ugr => ugr.UserGroup)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task AssignPolicyToRoleAsync(int roleId, int policyId)
    {
        _context.RolePolicies.Add(new RolePolicy { RoleId = roleId, PolicyId = policyId });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> PolicyAssignmentExistsAsync(int roleId, int policyId)
    {
        return await _context.RolePolicies
            .AnyAsync(rp => rp.RoleId == roleId && rp.PolicyId == policyId);
    }
}
