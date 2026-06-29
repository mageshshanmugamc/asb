namespace ASB.Repositories.v1.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
