using solDocs.Dtos.DashboardMetrics;

namespace solDocs.Interfaces
{
    public interface IMetricsService
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    }
}