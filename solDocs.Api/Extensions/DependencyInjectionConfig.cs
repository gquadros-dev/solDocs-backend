using solDocs.Interfaces;
using solDocs.Services;

namespace solDocs.Extensions
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFtpService, FtpService>();
            services.AddScoped<IMediaAssetService, MediaAssetService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IMetricsService, MetricsService>();

            return services;
        }
    }
}