using ASB.Services.v1.Implementations;
using ASB.Services.v1.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ASB.Services.v1;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPolicyService, PolicyService>();

        return services;
    }
}
