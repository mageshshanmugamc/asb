using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASB.Repositories.v1.Implementations
{
    public class UserGroupRepository : IUserGroupRepository
    {
        private readonly AsbContext _context;

        public UserGroupRepository(AsbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserGroup>> GetAllAsync()
        {
            return await _context.UserGroups
                .Include(ug => ug.UserGroupMappings)
                    .ThenInclude(ugm => ugm.User)
                .Include(ug => ug.UserGroupRoles)
                    .ThenInclude(ugr => ugr.Role)
                .ToListAsync();
        }

        public async Task<UserGroup?> GetByIdAsync(int id)
        {
            return await _context.UserGroups
                .Include(ug => ug.UserGroupMappings)
                    .ThenInclude(ugm => ugm.User)
                .Include(ug => ug.UserGroupRoles)
                    .ThenInclude(ugr => ugr.Role)
                .FirstOrDefaultAsync(ug => ug.Id == id);
        }

        public async Task<UserGroup> CreateAsync(UserGroup userGroup)
        {
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(userGroup.Id) ?? userGroup;
        }

        public async Task<UserGroup> UpdateAsync(UserGroup userGroup)
        {
            var existing = await _context.UserGroups
                .Include(ug => ug.UserGroupRoles)
                .FirstOrDefaultAsync(ug => ug.Id == userGroup.Id)
                ?? throw new KeyNotFoundException($"UserGroup with Id {userGroup.Id} not found.");

            existing.GroupName = userGroup.GroupName;

            // Replace role assignments
            _context.UserGroupRoles.RemoveRange(existing.UserGroupRoles);
            foreach (var ugr in userGroup.UserGroupRoles)
            {
                existing.UserGroupRoles.Add(new UserGroupRole
                {
                    UserGroupId = existing.Id,
                    RoleId = ugr.RoleId
                });
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(existing.Id) ?? existing;
        }

        public async Task AddUserToGroupAsync(int userId, int groupId)
        {
            var group = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.Id == groupId)
                ?? throw new KeyNotFoundException($"UserGroup with Id {groupId} not found.");

            var user = await _context.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException($"User with Id {userId} not found.");

            _context.UserGroupMappings.Add(new UserGroupMapping
            {
                UserId = userId,
                UserGroupId = groupId,
                AssignedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToGroupAsync(int groupId, int roleId)
        {
            _context.UserGroupRoles.Add(new UserGroupRole
            {
                UserGroupId = groupId,
                RoleId = roleId
            });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RoleAssignmentExistsAsync(int groupId, int roleId)
        {
            return await _context.UserGroupRoles
                .AnyAsync(ugr => ugr.UserGroupId == groupId && ugr.RoleId == roleId);
        }
    }
}
