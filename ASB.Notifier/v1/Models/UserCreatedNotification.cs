namespace ASB.Notifier.v1.Models;

public class UserCreatedNotification
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Message => $"New user '{Username}' has been created.";
}
