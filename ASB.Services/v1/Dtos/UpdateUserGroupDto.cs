namespace ASB.Services.v1.Dtos
{
    public class UpdateUserGroupDto
    {
        public required string GroupName { get; set; }
        public List<int> RoleIds { get; set; } = [];
    }
}
