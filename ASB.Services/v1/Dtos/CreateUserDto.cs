namespace ASB.Services.v1.Dtos
{
    public class CreateUserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public List<int> UserGroupIds { get; set; } = [];
    }
}
