namespace ASB.Repositories.v1.Models;

public class PaginationQuery
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
    public string? NameFilter { get; set; }
}
