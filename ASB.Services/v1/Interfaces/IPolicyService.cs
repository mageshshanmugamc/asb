using ASB.Services.v1.Dtos;

namespace ASB.Services.v1.Interfaces
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyDto>> GetAllAsync();
        Task<PolicyDto?> GetByIdAsync(int id);
        Task<PolicyDto> CreateAsync(CreatePolicyDto dto);
        Task<PolicyDto> UpdateAsync(int id, UpdatePolicyDto dto);
    }
}
