using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using solDocs.Interfaces;
using solDocs.Dtos;
using System.Security.Claims;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/{tenantSlug}/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ITenantService _tenantService;

        public SearchController(ISearchService searchService, ITenantService tenantService)
        {
            _searchService = searchService;
            _tenantService = tenantService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string tenantSlug, [FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Ok(new List<SearchResultDto>());
            }
            
            var tenant = await _tenantService.GetBySlugAsync(tenantSlug);
            if (tenant == null)
            {
                return NotFound(new { message = "Tenant não encontrado." });
            }

            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (isAuthenticated)
            {
                var tokenTenantId = User.FindFirstValue("tenantId");
                if (tokenTenantId != tenant.Id)
                {
                    return Forbid();
                }
            }

            var results = await _searchService.SearchAsync(text, tenant.Id, isAuthenticated);
            return Ok(results);
        }
    }
}