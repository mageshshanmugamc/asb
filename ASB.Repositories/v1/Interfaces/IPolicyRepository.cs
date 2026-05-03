using ASB.Repositories.v1.Entities;

namespace ASB.Repositories.v1.Interfaces
{
    public interface IPolicyRepository
    {
        Task<IEnumerable<Policy>> GetAllAsync();
        Task<Policy?> GetByIdAsync(int id);
        Task<Policy> CreateAsync(Policy policy);
        Task<Policy> UpdateAsync(Policy policy);
    }
}
