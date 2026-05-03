using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;

namespace ASB.Services.v1.Implementations
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyService(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<IEnumerable<PolicyDto>> GetAllAsync()
        {
            var policies = await _policyRepository.GetAllAsync();
            return policies.Select(MapToDto);
        }

        public async Task<PolicyDto?> GetByIdAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            return policy is null ? null : MapToDto(policy);
        }

        public async Task<PolicyDto> CreateAsync(CreatePolicyDto dto)
        {
            var policy = new Policy
            {
                Name = dto.Name,
                Description = dto.Description,
                Resource = dto.Resource,
                Action = dto.Action
            };
            var created = await _policyRepository.CreateAsync(policy);
            return MapToDto(created);
        }

        public async Task<PolicyDto> UpdateAsync(int id, UpdatePolicyDto dto)
        {
            var policy = new Policy
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Resource = dto.Resource,
                Action = dto.Action
            };
            var updated = await _policyRepository.UpdateAsync(policy);
            return MapToDto(updated);
        }

        private static PolicyDto MapToDto(Policy policy) => new()
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            Resource = policy.Resource,
            Action = policy.Action
        };
    }
}
