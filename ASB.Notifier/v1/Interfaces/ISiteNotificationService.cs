namespace ASB.Notifier.v1.Interfaces;

public interface ISiteNotificationService
{
    Task NotifySiteCreatedAsync(string siteName);
}
