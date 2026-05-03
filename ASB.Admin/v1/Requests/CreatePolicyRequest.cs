namespace ASB.Admin.v1.Requests
{
    public class CreatePolicyRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Resource { get; set; }
        public required string Action { get; set; }
    }
}
