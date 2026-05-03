namespace ASB.Repositories.v1.Entities
{
    /// <summary>
    /// Explicit join table between User and UserGroup with audit metadata.
    /// </summary>
    public class UserGroupMapping
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }
    }
}
