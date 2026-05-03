namespace ASB.Repositories.v1;

using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Implementations;
using ASB.Repositories.v1.Interfaces;
using ASB.Repositories.v1.Neo4j;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to register the database provider (SqlServer or Neo4j)
/// based on the "DatabaseProvider" configuration key.
/// </summary>
public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";
        Console.WriteLine($"Configuring database provider: {provider}");

        return provider.Equals("Neo4j", StringComparison.OrdinalIgnoreCase)
            ? services.AddNeo4jRepositories(configuration)
            : services.AddSqlServerRepositories(configuration);
    }

    private static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AsbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("AsbDatabase"),
                b => b.MigrationsAssembly("ASB.Admin")
                       .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserGroupRepository, UserGroupRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();

        return services;
    }

    private static IServiceCollection AddNeo4jRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var neo4jUri = configuration["Neo4j:Uri"]
            ?? throw new InvalidOperationException("Neo4j:Uri is not configured.");
        var neo4jUsername = configuration["Neo4j:Username"]
            ?? throw new InvalidOperationException("Neo4j:Username is not configured.");
        var neo4jPassword = configuration["Neo4j:Password"]
            ?? throw new InvalidOperationException("Neo4j:Password is not configured.");

        services.AddSingleton<INeo4jSessionFactory>(
            new Neo4jSessionFactory(neo4jUri, neo4jUsername, neo4jPassword));

        services.AddScoped<IUserRepository, Neo4jUserRepository>();
        services.AddScoped<IUserGroupRepository, Neo4jUserGroupRepository>();
        services.AddScoped<IMenuRepository, Neo4jMenuRepository>();
        services.AddScoped<IRoleRepository, Neo4jRoleRepository>();
        services.AddScoped<IPolicyRepository, Neo4jPolicyRepository>();

        return services;
    }
}
