using ASB.Repositories.v1.Entities;

namespace ASB.Repositories.v1.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> CreateAsync(Role role);
        Task AssignPolicyToRoleAsync(int roleId, int policyId);
        Task<bool> PolicyAssignmentExistsAsync(int roleId, int policyId);
    }
}
