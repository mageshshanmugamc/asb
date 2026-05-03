using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASB.Admin.v1.Infrastructure;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public KeycloakAdminService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<KeycloakUserInfo?> GetUserByEmailAsync(string email)
    {
        var baseUrl = _configuration["Keycloak:AdminBaseUrl"]
            ?? throw new InvalidOperationException("Keycloak:AdminBaseUrl not configured.");
        var realm = _configuration["Keycloak:AdminRealm"]
            ?? throw new InvalidOperationException("Keycloak:AdminRealm not configured.");

        var token = await GetAdminTokenAsync(baseUrl);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/admin/realms/{realm}/users?email={Uri.EscapeDataString(email)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var kcUser = users?.FirstOrDefault();
        if (kcUser == null) return null;

        return new KeycloakUserInfo
        {
            Id = kcUser.Id,
            Username = kcUser.Username,
            Email = kcUser.Email,
            FirstName = kcUser.FirstName ?? string.Empty,
            LastName = kcUser.LastName ?? string.Empty
        };
    }

    private async Task<string> GetAdminTokenAsync(string baseUrl)
    {
        var adminUser = _configuration["Keycloak:AdminUsername"] ?? "admin";
        var adminPass = _configuration["Keycloak:AdminPassword"] ?? "admin";

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
            $"{baseUrl}/realms/master/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = adminUser,
                ["password"] = adminPass
            })
        };

        var response = await _httpClient.SendAsync(tokenRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
        return tokenResponse.GetProperty("access_token").GetString()!;
    }

    private class KeycloakUserRepresentation
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
