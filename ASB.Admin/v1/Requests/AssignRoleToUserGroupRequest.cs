namespace ASB.Admin.v1.Requests
{
    public class AssignRoleToUserGroupRequest
    {
        public required int UserGroupId { get; set; }
        public required int RoleId { get; set; }
    }
}
