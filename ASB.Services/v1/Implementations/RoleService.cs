using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;

namespace ASB.Services.v1.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapToDto);
        }

        public async Task<RoleDto?> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role is null ? null : MapToDto(role);
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
        {
            var role = new Role { Name = dto.Name };
            var created = await _roleRepository.CreateAsync(role);
            return MapToDto(created);
        }

        public async Task AssignPolicyToRoleAsync(int roleId, int policyId)
        {
            var exists = await _roleRepository.PolicyAssignmentExistsAsync(roleId, policyId);
            if (exists)
                throw new InvalidOperationException($"Policy {policyId} is already assigned to role {roleId}.");

            await _roleRepository.AssignPolicyToRoleAsync(roleId, policyId);
        }

        private static RoleDto MapToDto(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name
        };
    }
}
