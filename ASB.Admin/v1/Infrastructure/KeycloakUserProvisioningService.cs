using System.Security.Claims;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;

namespace ASB.Admin.v1.Infrastructure
{
    /// <summary>
    /// Called on every validated JWT token.
    /// If the bearer's email is not yet in the database the user is created and
    /// automatically assigned to the "Viewer" group (read-only access by default).
    /// </summary>
    public class KeycloakUserProvisioningService : IKeycloakUserProvisioningService
    {
        private readonly IUserService _userService;
        private readonly ILogger<KeycloakUserProvisioningService> _logger;

        public KeycloakUserProvisioningService(
            IUserService userService,
            ILogger<KeycloakUserProvisioningService> logger)
        {
            _userService = userService;
            _logger      = logger;
        }

        public async Task ProvisionAsync(ClaimsPrincipal principal)
        {
            // Keycloak places email in the "email" claim; fall back to standard ClaimTypes
            var email    = principal.FindFirstValue("email")
                        ?? principal.FindFirstValue(ClaimTypes.Email);

            var username = principal.FindFirstValue("preferred_username")
                        ?? principal.FindFirstValue(ClaimTypes.Name)
                        ?? email;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Keycloak token has no email claim – skipping user provisioning.");
                return;
            }

            try
            {
                var dto = new CreateUserDto
                {
                    Username = username!,
                    Email = email,
                    UserGroupId = 4 // Viewer group by default
                };

                await _userService.CreateUserAsync(dto);
                _logger.LogInformation("Provisioned new user {Username} ({Email}) into Viewer group.", username, email);
            }
            catch (InvalidOperationException)
            {
                // User already exists — idempotent, nothing to do
            }
        }
    }
}
