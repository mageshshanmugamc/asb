namespace ASB.Admin.v1.Requests
{
    public class CreateCountryRequest
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string Market { get; set; }
    }
}
