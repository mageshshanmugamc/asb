using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ASB.Services.v1.Implementations;

public class AuthTokenService : IAuthTokenService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthTokenService(
        IMenuRepository menuRepository,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _menuRepository = menuRepository;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AppTokenDto> GenerateAppTokenAsync(GenerateTokenDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email)
            ?? throw new KeyNotFoundException($"User with email '{dto.Email}' not found.");

        // Get roles via groups
        var roleIds = await _menuRepository.GetUserRoleIdsAsync(user.Id);

        // Get allowed menus for those roles
        var menus = await _menuRepository.GetMenusByRoleIdsAsync(roleIds);

        // Build role names from the role entities (via group roles)
        var roleNames = menus.SelectMany(m => m.RoleMenuPermissions)
            .Where(rmp => roleIds.Contains(rmp.RoleId))
            .Select(rmp => rmp.Role.Name)
            .Distinct()
            .ToList();

        // If no role names resolved from menus, fetch them directly
        if (!roleNames.Any())
        {
            roleNames = roleIds.Select(id => $"role_{id}").ToList();
        }

        // Get policy names for the user's roles
        var policyNames = await _menuRepository.GetPolicyNamesByRoleIdsAsync(roleIds);

        // Build JWT
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var menuDtos = BuildMenuHierarchy(menus);
        var token = GenerateJwt(user, roleNames, policyNames, menuDtos, expiresAt);

        return new AppTokenDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Roles = roleNames,
            Menus = menuDtos
        };
    }

    private string GenerateJwt(User user, List<string> roles, List<string> permissions, List<MenuDto> menus, DateTime expiresAt)
    {
        var key = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "asb-api";
        var audience = _configuration["Jwt:Audience"] ?? "asb-client";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("preferred_username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("menus", JsonSerializer.Serialize(menus, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<MenuDto> BuildMenuHierarchy(List<Menu> menus)
    {
        var lookup = menus.ToDictionary(m => m.Id);
        var roots = new List<MenuDto>();

        foreach (var menu in menus.OrderBy(m => m.DisplayOrder))
        {
            var dto = new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Route = menu.Route,
                Icon = menu.Icon,
                DisplayOrder = menu.DisplayOrder
            };

            if (menu.ParentMenuId == null || !lookup.ContainsKey(menu.ParentMenuId.Value))
            {
                roots.Add(dto);
            }
        }

        // Attach children
        foreach (var menu in menus.Where(m => m.ParentMenuId != null).OrderBy(m => m.DisplayOrder))
        {
            var parent = roots.FirstOrDefault(r => r.Id == menu.ParentMenuId);
            if (parent != null)
            {
                parent.Children.Add(new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Route = menu.Route,
                    Icon = menu.Icon,
                    DisplayOrder = menu.DisplayOrder
                });
            }
        }

        return roots;
    }
}
