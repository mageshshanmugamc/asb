namespace ASB.Admin.v1.Controllers
{
    using ASB.Admin.v1.Extensions;
    using ASB.Admin.v1.Requests;
    using ASB.Admin.v1.Response;
    using ASB.Authorization;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;

    /// <summary>
    /// Controller for managing sites.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [AsbAuthorize]
    [EnableRateLimiting(RateLimitingExtensions.FixedPolicy)]
    public class SitesController(ISiteService siteService) : ControllerBase
    {
        private readonly ISiteService siteService = siteService;

        [HttpGet]
        [AsbAuthorize(Policies.ReadOnly)]
        public async Task<IActionResult> GetSites([FromQuery] PaginatedQueryParams query)
        {
            var result = await siteService.GetSitesAsync(query.ToPaginationQuery());
            return Ok(new PagedResponse<SiteResponse>
            {
                Items = result.Items.Select(SiteResponse.DtoToSites),
                TotalCount = result.TotalCount,
                Skip = query.Skip,
                Take = query.Take
            });
        }
        [HttpPost]
        [AsbAuthorize(Policies.FullAccess)]
        public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequest request)
            {
                var dto = new CreateSiteDto
                {
                    Name = request.Name,
                    Location = request.Location
                };

                var site = await siteService.CreateSiteAsync(dto);
                return CreatedAtAction(nameof(GetSites), new { id = site.Id }, SiteResponse.DtoToSites(site));
            }
    }
}
