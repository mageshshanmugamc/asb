namespace ASB.Admin.v1.Requests;

using ASB.Repositories.v1.Models;
using Microsoft.AspNetCore.Mvc;

public class PaginatedQueryParams
{
    [FromQuery(Name = "skip")]
    public int Skip { get; set; } = 0;

    [FromQuery(Name = "take")]
    public int Take { get; set; } = 10;

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; set; }

    [FromQuery(Name = "sortDirection")]
    public string SortDirection { get; set; } = "asc";

    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    public bool IsDescending => SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

    public PaginationQuery ToPaginationQuery() => new()
    {
        Skip = Skip,
        Take = Take,
        SortBy = SortBy,
        IsDescending = IsDescending,
        NameFilter = Name
    };
}
