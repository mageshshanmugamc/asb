using ASB.Services.v1.Dtos;

namespace ASB.Services.v1.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(int id);
        Task<RoleDto> CreateAsync(CreateRoleDto dto);
        Task AssignPolicyToRoleAsync(int roleId, int policyId);
    }
}
