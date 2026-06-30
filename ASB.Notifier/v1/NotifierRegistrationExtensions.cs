namespace ASB.Notifier.v1;

using ASB.Notifier.v1.Implementations;
using ASB.Notifier.v1.Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering notifier services in the dependency injection container.
/// </summary>
public static class NotifierRegistrationExtensions
{
    /// <summary>
    /// Registers the notifier services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register the services in.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNotifierServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        return services;
    }
}
