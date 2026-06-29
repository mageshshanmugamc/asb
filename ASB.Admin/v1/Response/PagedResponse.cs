namespace ASB.Admin.v1.Response;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public bool HasMore => Skip + Take < TotalCount;
}
