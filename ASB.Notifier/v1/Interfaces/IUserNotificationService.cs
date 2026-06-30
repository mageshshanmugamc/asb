namespace ASB.Notifier.v1.Interfaces;

using ASB.Notifier.v1.Models;

/// <summary>
/// Interface for user notification service.
/// </summary>
public interface IUserNotificationService
{
    Task NotifyUserCreatedAsync(UserCreatedNotification notification);
    Task NotifyUserDeletedAsync(UserDeletedNotification notification);
}
