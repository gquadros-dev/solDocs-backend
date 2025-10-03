using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using solDocs.Interfaces;
using solDocs.Dtos.DashboardMetrics;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    [Authorize(Roles = "super_admin")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public MetricsController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Retorna um resumo de métricas do dashboard para o Super Admin.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics()
        {
            var metrics = await _metricsService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }
    }
}