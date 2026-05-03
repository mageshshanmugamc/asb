namespace ASB.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to register policy-based authorization.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddPolicyAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.FullAccess, policy =>
                policy.Requirements.Add(new PermissionRequirement(Policies.FullAccess)))
            .AddPolicy(Policies.ReadOnly, policy =>
                policy.Requirements.Add(new PermissionRequirement(Policies.ReadOnly)))
            .AddPolicy(Policies.ManageUsers, policy =>
                policy.Requirements.Add(new PermissionRequirement(Policies.ManageUsers)))
            .AddPolicy(Policies.AuditAccess, policy =>
                policy.Requirements.Add(new PermissionRequirement(Policies.AuditAccess)));

        return services;
    }
}
