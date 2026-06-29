namespace ASB.Notifier.v1.Interfaces;

using ASB.Notifier.v1.Models;

public interface INotificationService
{
    Task NotifyUserCreatedAsync(UserCreatedNotification notification);
}
