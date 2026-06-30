namespace ASB.Notifier.v1.Models;

/// <summary>
/// Represents a notification for a deleted user. 
/// </summary>
public class UserDeletedNotification
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string Message => $"User '{Username}' has been deleted.";
}
