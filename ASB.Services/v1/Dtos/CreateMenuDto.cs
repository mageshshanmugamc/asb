namespace ASB.Services.v1.Dtos
{
    public class CreateMenuDto
    {
        public required string Name { get; set; }
        public required string Route { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentMenuId { get; set; }
    }
}
