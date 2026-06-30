using ASB.Notifier.v1.Models;

namespace ASB.Notifier.v1.Interfaces;

/// <summary>
/// Defines the contract for a service that handles site-related notifications.
/// </summary>
public interface ISiteNotificationService
{
    /// <summary>
    /// Notifies that a site has been created.
    /// </summary>
    /// <param name="notification">The notification details.</param>
    Task NotifySiteCreatedAsync(SiteCreatedNotification notification);
}
