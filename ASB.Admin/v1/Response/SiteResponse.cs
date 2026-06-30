namespace ASB.Admin.v1.Response;

using ASB.Services.v1.Dtos;

public class SiteResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public static SiteResponse DtoToSites(SiteDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Location = dto.Location
    };
}
