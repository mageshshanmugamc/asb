using ASB.Services.v1.Dtos;

namespace ASB.Services.v1.Interfaces
{
    public interface IUserGroupService
    {
        Task<IEnumerable<UserGroupDto>> GetAllAsync();
        Task<UserGroupDto?> GetByIdAsync(int id);
        Task<UserGroupDto> CreateAsync(CreateUserGroupDto dto);
        Task AddUserToGroupAsync(int userId, int groupId);
        Task AssignRoleToGroupAsync(int groupId, int roleId);
    }
}
