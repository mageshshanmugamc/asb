namespace ASB.Notifier.v1.Implementations;

using ASB.Notifier.v1.Hubs;
using ASB.Notifier.v1.Interfaces;
using ASB.Notifier.v1.Models;
using Microsoft.AspNetCore.SignalR;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserCreatedAsync(UserCreatedNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("UserCreated", notification);
    }
}
