namespace ASB.Admin.v1.Infrastructure;

public class KeycloakUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public interface IKeycloakAdminService
{
    Task<KeycloakUserInfo?> GetUserByEmailAsync(string email);
}
