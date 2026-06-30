using ASB.Notifier.v1.Hubs;
using ASB.Notifier.v1.Interfaces;
using ASB.Notifier.v1.Models;
using Microsoft.AspNetCore.SignalR;

namespace ASB.Notifier.v1.Implementations;
/// <summary>
/// Initializes a new instance of the <see cref="UserNotificationService"/> class.
/// </summary>
/// <param name="hubContext"></param>
public class UserNotificationService(IHubContext<NotificationHub> hubContext) : IUserNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public async Task NotifyUserCreatedAsync(UserCreatedNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("UserCreated", notification);
    }

    public async Task NotifyUserDeletedAsync(UserDeletedNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("UserDeleted", notification);
    }
}
