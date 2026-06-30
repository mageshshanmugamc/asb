using ASB.Services.v1.Dtos;
using ASB.Repositories.v1.Models;

namespace ASB.Services.v1.Interfaces
{
    /// <summary>
    /// Defines the contract for site-related operations.
    /// </summary>
    public interface ISiteService
    {
        Task<IEnumerable<SiteDto>> GetSites();
        Task<PagedResult<SiteDto>> GetSitesAsync(PaginationQuery query);
        Task<SiteDto?> GetSiteByIdAsync(int id);
        Task<SiteDto> CreateSiteAsync(CreateSiteDto dto);
        Task UpdateSiteAsync(int id, UpdateSiteDto dto);
        Task DeleteSiteAsync(int id);
    }
}