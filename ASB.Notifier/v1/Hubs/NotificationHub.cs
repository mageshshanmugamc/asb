namespace ASB.Notifier.v1.Hubs;

using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Represents a SignalR hub for notifications.
/// </summary>
/// <remarks>
/// This hub is used to send real-time notifications to connected clients.
/// </remarks>
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
