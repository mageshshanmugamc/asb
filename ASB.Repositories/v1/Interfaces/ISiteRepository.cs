using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Models;

namespace ASB.Repositories.v1.Interfaces;

public interface ISiteRepository
{
    Task<IEnumerable<Site>> GetAllSitesAsync();
    Task<PagedResult<Site>> GetAllSitesAsync(PaginationQuery query);
    Task<Site?> GetSiteByIdAsync(int id);
    Task<Site> CreateSiteAsync(Site site);
    Task UpdateSiteAsync(Site site);
    Task DeleteSiteAsync(Site site);
}
