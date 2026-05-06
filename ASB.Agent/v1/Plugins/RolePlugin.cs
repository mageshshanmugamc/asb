using ASB.Services.v1.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ASB.Agent.v1.Plugins;

/// <summary>
/// Semantic Kernel plugin that exposes Role management operations as tools.
/// </summary>
public class RolePlugin
{
    private readonly IRoleService _roleService;

    public RolePlugin(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [KernelFunction("get_all_roles")]
    [Description("Retrieves a list of all roles in the system with their IDs, names, and descriptions.")]
    public async Task<string> GetAllRolesAsync()
    {
        var roles = await _roleService.GetAllAsync();
        var summary = roles.Select(r => $"- {r.Name} (ID: {r.Id})");
        return string.Join("\n", summary);
    }

    [KernelFunction("get_role_by_id")]
    [Description("Gets detailed information about a specific role by its integer ID.")]
    public async Task<string> GetRoleByIdAsync(
        [Description("The integer ID of the role")] int roleId)
    {
        var role = await _roleService.GetByIdAsync(roleId);
        if (role == null)
            return $"No role found with ID {roleId}";

        return $"Role: {role.Name} (ID: {role.Id})";
    }
}
