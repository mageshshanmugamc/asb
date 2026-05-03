namespace ASB.Repositories.v1.Entities
{
    public class UserGroupRole
    {
        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
