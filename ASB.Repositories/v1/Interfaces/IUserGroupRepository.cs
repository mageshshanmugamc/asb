using ASB.Repositories.v1.Entities;

namespace ASB.Repositories.v1.Interfaces
{
    public interface IUserGroupRepository
    {
        Task<IEnumerable<UserGroup>> GetAllAsync();
        Task<UserGroup?> GetByIdAsync(int id);
        Task<UserGroup> CreateAsync(UserGroup userGroup);
        Task AddUserToGroupAsync(int userId, int groupId);
        Task AssignRoleToGroupAsync(int groupId, int roleId);
        Task<bool> RoleAssignmentExistsAsync(int groupId, int roleId);
    }
}
