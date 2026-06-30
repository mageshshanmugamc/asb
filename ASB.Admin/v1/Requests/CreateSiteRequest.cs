namespace ASB.Admin.v1.Requests;
public class CreateSiteRequest
{
    public required string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}