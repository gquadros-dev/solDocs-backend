namespace solDocs.Dtos.DashboardMetrics
{
    public class DashboardMetricsDto
    {
        public long TotalTenants { get; set; }
        public long ActiveTenants { get; set; }
        public IEnumerable<TenantMetricDto> TenantsByArticleCount { get; set; } = new List<TenantMetricDto>();
        public IEnumerable<TenantMetricDto> TenantsNearingUserLimit { get; set; } = new List<TenantMetricDto>();
    }
}