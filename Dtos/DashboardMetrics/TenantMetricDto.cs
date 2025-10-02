namespace solDocs.Dtos.DashboardMetrics
{
    public class TenantMetricDto
    {
        public string TenantId { get; set; } = null!;
        public string TenantName { get; set; } = null!;
        public long Value { get; set; }
    }
}