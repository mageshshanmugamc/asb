namespace ASB.Notifier.v1.Implementations;

using ASB.Notifier.v1.Hubs;
using ASB.Notifier.v1.Interfaces;
using ASB.Notifier.v1.Models;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Represents a service that handles site-related notifications using SignalR.
/// </summary>
/// <param name="hubContext">The SignalR hub context used to send notifications.</param>
public class SiteNotificationService(IHubContext<NotificationHub> hubContext) : ISiteNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public async Task NotifySiteCreatedAsync(SiteCreatedNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("SiteCreated", notification);
    }
}
