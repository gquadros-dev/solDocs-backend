using MongoDB.Driver;
using Microsoft.Extensions.Options;
using solDocs.Models;

namespace solDocs.Extensions
{
    public static class MongoDbConfig
    {
        public static IServiceCollection AddMongoDbConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration.GetConnectionString("MongoDbConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                    connectionString = settings.ConnectionString;
                }

                return new MongoClient(connectionString);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            return services;
        }
    }
}