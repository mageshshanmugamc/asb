using System.Security.Claims;

namespace ASB.Admin.v1.Infrastructure
{
    /// <summary>
    /// Provisions a local database record for a Keycloak-authenticated user on their
    /// first request. Subsequent requests are no-ops once the user record exists.
    /// </summary>
    public interface IKeycloakUserProvisioningService
    {
        Task ProvisionAsync(ClaimsPrincipal principal);
    }
}
