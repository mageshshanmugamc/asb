namespace ASB.Repositories.v1.Entities
{
    public class RolePolicy
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public int PolicyId { get; set; }
        public Policy Policy { get; set; } = null!;
    }
}