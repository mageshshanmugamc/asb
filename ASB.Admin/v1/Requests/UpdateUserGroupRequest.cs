namespace ASB.Admin.v1.Requests
{
    public class UpdateUserGroupRequest
    {
        public required string GroupName { get; set; }
        public List<int> RoleIds { get; set; } = [];
    }
}
