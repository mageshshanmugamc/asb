namespace ASB.Repositories.v1.Entities
{
    /// <summary>
    /// Represents a user group that can contain multiple users. User groups are used to organize users into logical units for easier management and access control. Each user group can have multiple users associated with it, allowing for group-based permissions and policies to be applied to all members of the group.
    /// </summary>
    public class UserGroup
    {
        /// <summary>
        /// The unique identifier for the user group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the user group.
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// The collection of user-group memberships.
        /// </summary>
        public ICollection<UserGroupMapping> UserGroupMappings { get; set; } = [];

        /// <summary>
        /// The collection of roles assigned to this user group.
        /// </summary>
        public ICollection<UserGroupRole> UserGroupRoles { get; set; } = [];
    }
}