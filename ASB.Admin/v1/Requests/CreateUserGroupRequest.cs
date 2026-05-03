namespace ASB.Admin.v1.Requests
{
    public class CreateUserGroupRequest
    {
        public required string GroupName { get; set; }
        public List<int> RoleIds { get; set; } = [];
    }
}
