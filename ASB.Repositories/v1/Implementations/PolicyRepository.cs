using Microsoft.EntityFrameworkCore;
using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;

namespace ASB.Repositories.v1.Implementations;

public class PolicyRepository : IPolicyRepository
{
    private readonly AsbContext _context;

    public PolicyRepository(AsbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Policy>> GetAllAsync()
    {
        return await _context.Policies.ToListAsync();
    }

    public async Task<Policy?> GetByIdAsync(int id)
    {
        return await _context.Policies.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Policy> CreateAsync(Policy policy)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy)
    {
        var existing = await _context.Policies
            .FirstOrDefaultAsync(p => p.Id == policy.Id)
            ?? throw new KeyNotFoundException($"Policy with Id {policy.Id} not found.");

        existing.Name = policy.Name;
        existing.Description = policy.Description;
        existing.Resource = policy.Resource;
        existing.Action = policy.Action;

        await _context.SaveChangesAsync();
        return existing;
    }
}
