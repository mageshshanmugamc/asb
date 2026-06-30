namespace ASB.Notifier.v1.Models;

/// <summary>
/// Represents a notification for a created site.
/// </summary>
public class SiteCreatedNotification
{
    public required string SiteName { get; set; }
    public string Message => $"Site '{SiteName}' has been created.";
}
