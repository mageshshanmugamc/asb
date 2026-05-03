namespace ASB.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Handles PermissionRequirement by checking the user's "permissions" claims.
/// A user with "FullAccess" permission passes all policy checks.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions").Select(c => c.Value);

        if (permissions.Contains("FullAccess") || permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
