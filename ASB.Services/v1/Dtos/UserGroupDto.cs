namespace ASB.Services.v1.Dtos
{
    public class UserGroupDto
    {
        public int Id { get; set; }
        public required string GroupName { get; set; }
        public List<UserDto> Users { get; set; } = [];
        public List<RoleDto> Roles { get; set; } = [];
    }
}
