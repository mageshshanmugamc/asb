namespace ASB.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Requirement that checks whether the user has a specific permission claim.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
