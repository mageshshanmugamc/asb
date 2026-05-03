namespace ASB.Repositories.v1.Entities
{
    /// <summary>
    /// Represents a user in the system. A user can have multiple roles and can be part of multiple groups. The User entity contains properties for the user's unique identifier, username, email, and password hash. It also includes collections for the user's role mappings and group memberships, allowing for flexible assignment of permissions and access control within the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The hashed password of the user.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// The collection of group memberships for this user.
        /// </summary>
        public ICollection<UserGroupMapping> UserGroupMappings { get; set; } = [];
    }
}