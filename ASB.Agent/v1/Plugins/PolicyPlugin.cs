using ASB.Services.v1.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ASB.Agent.v1.Plugins;

/// <summary>
/// Semantic Kernel plugin that exposes Policy operations as tools.
/// </summary>
public class PolicyPlugin
{
    private readonly IPolicyService _policyService;

    public PolicyPlugin(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [KernelFunction("get_all_policies")]
    [Description("Retrieves all policies (permissions) available in the system.")]
    public async Task<string> GetAllPoliciesAsync()
    {
        var policies = await _policyService.GetAllAsync();
        var summary = policies.Select(p => $"- {p.Name} (ID: {p.Id}): {p.Description}");
        return string.Join("\n", summary);
    }
}
