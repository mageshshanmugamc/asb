namespace ASB.Admin.v1.Requests
{
    public class CreateMenuRequest
    {
        public required string Name { get; set; }
        public required string Route { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentMenuId { get; set; }
    }
}
