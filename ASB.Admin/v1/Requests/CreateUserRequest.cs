namespace ASB.Admin.v1.Requests
{
    public class CreateUserRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public List<int> UserGroupIds { get; set; } = [];
    }
}
