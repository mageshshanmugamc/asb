namespace ASB.Services.v1.Implementations
{
    using System.Collections.Generic;
    using ASB.Notifier.v1.Interfaces;
    using ASB.Notifier.v1.Models;
    using ASB.Repositories.v1.Entities;
    using ASB.Repositories.v1.Interfaces;
    using ASB.Repositories.v1.Models;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;

    /// <summary>
    /// Implementation of the ISiteService interface.
    /// </summary>
    /// <param name="siteRepository">The site repository.</param>
    /// <param name="siteNotificationService">The site notification service.</param>
    public class SiteService(ISiteRepository siteRepository, ISiteNotificationService siteNotificationService) : ISiteService
    {
        private readonly ISiteRepository _siteRepository = siteRepository;
        private readonly ISiteNotificationService _siteNotificationService = siteNotificationService;

        public async Task<IEnumerable<SiteDto>> GetSites()
        {
            var sites = await _siteRepository.GetAllSitesAsync();
            return sites.Select(site => new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Location = site.Location
            });
        }

        public async Task<PagedResult<SiteDto>> GetSitesAsync(PaginationQuery query)
        {
            var result = await _siteRepository.GetAllSitesAsync(query);
            return new PagedResult<SiteDto>
            {
                TotalCount = result.TotalCount,
                Items = result.Items.Select(site => new SiteDto
                {
                    Id = site.Id,
                    Name = site.Name,
                    Location = site.Location
                })
            };
        }

        public async Task<SiteDto> CreateSiteAsync(CreateSiteDto dto)
        {
            var site = new Site
            {
                Name = dto.Name,
                Location = dto.Location
            };

            var createdSite = await _siteRepository.CreateSiteAsync(site);
            await _siteNotificationService.NotifySiteCreatedAsync(new SiteCreatedNotification
            {
                SiteName = createdSite.Name
            });

            return new SiteDto
            {
                Id = createdSite.Id,
                Name = createdSite.Name,
                Location = createdSite.Location
            };
        }

        public async Task<SiteDto?> GetSiteByIdAsync(int id)
        {
            var site = await _siteRepository.GetSiteByIdAsync(id);
            if (site == null)
            {
                return null;
            }

            return new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Location = site.Location
            };
        }

        public async Task UpdateSiteAsync(int id, UpdateSiteDto dto)
        {
            var site = await _siteRepository.GetSiteByIdAsync(id);
            if (site == null)
            {
                throw new KeyNotFoundException($"Site with ID {id} not found.");
            }

            site.Name = dto.Name;
            site.Location = dto.Location;

            await _siteRepository.UpdateSiteAsync(site);
        }

        public async Task DeleteSiteAsync(int id)
        {
            var site = await _siteRepository.GetSiteByIdAsync(id);
            if (site == null)
            {
                throw new KeyNotFoundException($"Site with ID {id} not found.");
            }

            await _siteRepository.DeleteSiteAsync(site);
        }
    }
}