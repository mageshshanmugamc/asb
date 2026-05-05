namespace ASB.Authorization;

using ASB.ErrorHandler.v1.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Custom authorization attribute that validates authentication and permission claims,
/// throwing exceptions handled by the centralized error handler instead of returning
/// default 401/403 challenge responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AsbAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string? Policy { get; }

    public AsbAuthorizeAttribute()
    {
    }

    public AsbAuthorizeAttribute(string policy)
    {
        Policy = policy;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Allow endpoints marked with [AllowAnonymous] to bypass
        if (context.ActionDescriptor.EndpointMetadata
            .Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute))
        {
            return;
        }

        var user = context.HttpContext.User;

        // Check authentication
        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            throw new UnAuthorizedException("Authentication is required to access this resource.");
        }

        // If no policy specified, authenticated access is sufficient
        if (string.IsNullOrEmpty(Policy))
        {
            return;
        }

        // Check permission claims
        var permissions = user.FindAll("permissions").Select(c => c.Value).ToList();

        if (permissions.Count == 0)
        {
            throw new ForbiddenAccessException("Access denied. No permissions found for the current user.");
        }

        if (permissions.Contains(Policies.FullAccess))
        {
            return;
        }

        if (!permissions.Contains(Policy))
        {
            throw new ForbiddenAccessException(
                $"Access denied. Required permission '{Policy}' is not granted to the current user.");
        }
    }
}
