namespace ASB.Services.v1.Dtos;

public class AppTokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<MenuDto> Menus { get; set; } = [];
    public List<string> Roles { get; set; } = [];
}
