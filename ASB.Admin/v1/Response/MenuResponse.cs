namespace ASB.Admin.v1.Response
{
    using ASB.Services.v1.Dtos;

    public class MenuResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuResponse> Children { get; set; } = [];

        public static MenuResponse FromDto(MenuDto dto)
        {
            return new MenuResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                Route = dto.Route,
                Icon = dto.Icon,
                DisplayOrder = dto.DisplayOrder,
                Children = dto.Children.Select(FromDto).ToList()
            };
        }

        public static IEnumerable<MenuResponse> FromDtoList(IEnumerable<MenuDto> dtos)
        {
            return dtos.Select(FromDto);
        }
    }
}
