using MongoDB.Driver;
using solDocs.Dtos.DashboardMetrics;
using solDocs.Models;
using solDocs.Interfaces;

namespace solDocs.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly IMongoCollection<TenantModel> _tenants;
        private readonly IMongoCollection<ArticleModel> _articles;
        private readonly IMongoCollection<UserModel> _users;

        public MetricsService(IMongoDatabase database)
        {
            _tenants = database.GetCollection<TenantModel>("tenants");
            _articles = database.GetCollection<ArticleModel>("Articles");
            _users = database.GetCollection<UserModel>("Users");
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        {
            var totalTenantsTask = _tenants.CountDocumentsAsync(t => !t.Deletado);
            var activeTenantsTask = _tenants.CountDocumentsAsync(t => !t.Deletado && t.Ativo);
            var tenantsByArticleCountTask = GetTenantsByArticleCountAsync();
            var tenantsNearingUserLimitTask = GetTenantsNearingUserLimitAsync();

            await Task.WhenAll(
                totalTenantsTask, 
                activeTenantsTask, 
                tenantsByArticleCountTask, 
                tenantsNearingUserLimitTask
            );

            return new DashboardMetricsDto
            {
                TotalTenants = await totalTenantsTask,
                ActiveTenants = await activeTenantsTask,
                TenantsByArticleCount = await tenantsByArticleCountTask,
                TenantsNearingUserLimit = await tenantsNearingUserLimitTask
            };
        }

        private async Task<List<TenantMetricDto>> GetTenantsByArticleCountAsync()
        {
            var articleCounts = await _articles.Aggregate()
                .Match(a => a.DeletedAt == null) 
                .Group(a => a.TenantId, g => new { TenantId = g.Key, Value = g.Count() })
                .SortByDescending(g => g.Value) 
                .Limit(10) 
                .ToListAsync();

            var tenantIds = articleCounts.Select(ac => ac.TenantId).ToList();
            var tenants = await _tenants.Find(t => tenantIds.Contains(t.Id)).ToListAsync();
            var tenantMap = tenants.ToDictionary(t => t.Id, t => t.Nome);

            return articleCounts.Select(ac => new TenantMetricDto
            {
                TenantId = ac.TenantId,
                TenantName = tenantMap.ContainsKey(ac.TenantId) ? tenantMap[ac.TenantId] : "Nome não encontrado",
                Value = ac.Value
            }).ToList();
        }

        private async Task<List<TenantMetricDto>> GetTenantsNearingUserLimitAsync()
        {
            var userCounts = await _users.Aggregate()
                .Group(u => u.TenantId, g => new { TenantId = g.Key, UserCount = g.Count() })
                .ToListAsync();

            var tenants = await _tenants.Find(t => !t.Deletado && t.Ativo).ToListAsync();
            var tenantsNearingLimit = new List<TenantMetricDto>();

            foreach (var tenant in tenants)
            {
                var userCount = userCounts.FirstOrDefault(uc => uc.TenantId == tenant.Id)?.UserCount ?? 0;
                if (userCount > 0 && tenant.LimiteUsuarios > 0 && (double)userCount / tenant.LimiteUsuarios >= 0.9)
                {
                    tenantsNearingLimit.Add(new TenantMetricDto
                    {
                        TenantId = tenant.Id,
                        TenantName = tenant.Nome,
                        Value = userCount
                    });
                }
            }

            return tenantsNearingLimit.OrderByDescending(t => t.Value).ToList();
        }
    }
}