using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;

namespace ASB.Services.v1.Implementations
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUserGroupRepository _userGroupRepository;

        public UserGroupService(IUserGroupRepository userGroupRepository)
        {
            _userGroupRepository = userGroupRepository;
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllAsync()
        {
            var groups = await _userGroupRepository.GetAllAsync();
            return groups.Select(MapToDto);
        }

        public async Task<UserGroupDto?> GetByIdAsync(int id)
        {
            var group = await _userGroupRepository.GetByIdAsync(id);
            return group is null ? null : MapToDto(group);
        }

        public async Task<UserGroupDto> CreateAsync(CreateUserGroupDto dto)
        {
            var group = new UserGroup { GroupName = dto.GroupName };
            var created = await _userGroupRepository.CreateAsync(group);
            return MapToDto(created);
        }

        public async Task AddUserToGroupAsync(int userId, int groupId)
        {
            await _userGroupRepository.AddUserToGroupAsync(userId, groupId);
        }

        public async Task AssignRoleToGroupAsync(int groupId, int roleId)
        {
            var exists = await _userGroupRepository.RoleAssignmentExistsAsync(groupId, roleId);
            if (exists)
                throw new InvalidOperationException($"Role {roleId} is already assigned to group {groupId}.");

            await _userGroupRepository.AssignRoleToGroupAsync(groupId, roleId);
        }

        private static UserGroupDto MapToDto(UserGroup group)
        {
            return new UserGroupDto
            {
                Id = group.Id,
                GroupName = group.GroupName,
                Users = group.UserGroupMappings.Select(ugm => new UserDto
                {
                    Id = ugm.User.Id,
                    Username = ugm.User.Username,
                    Email = ugm.User.Email
                }).ToList(),
                Roles = group.UserGroupRoles.Select(ugr => new RoleDto
                {
                    Id = ugr.Role.Id,
                    Name = ugr.Role.Name
                }).ToList()
            };
        }
    }
}
