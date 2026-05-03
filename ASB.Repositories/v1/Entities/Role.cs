namespace ASB.Repositories.v1.Entities
{
    /// <summary>
    /// Represents a role that can be assigned to users or groups. Roles define a set of permissions that determine what actions a user or group can perform on resources. Each role can have multiple policies associated with it, which specify the permissions and rules for that role.
    /// </summary>
   public class Role
    {
        /// <summary>
        /// The unique identifier for the role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The collection of policies associated with the role.
        /// </summary>
        public ICollection<RolePolicy> RolePolicies { get; set; } = [];

        /// <summary>
        /// The collection of user groups this role is assigned to.
        /// </summary>
        public ICollection<UserGroupRole> UserGroupRoles { get; set; } = [];

        /// <summary>
        /// The collection of menu permissions associated with this role.
        /// </summary>
        public ICollection<RoleMenuPermission> RoleMenuPermissions { get; set; } = [];
    }
}