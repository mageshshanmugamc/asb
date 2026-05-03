namespace ASB.Services.v1.Dtos
{
    public class CreatePolicyDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Resource { get; set; }
        public required string Action { get; set; }
    }
}
