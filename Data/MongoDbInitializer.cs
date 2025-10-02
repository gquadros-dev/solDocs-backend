using solDocs.Models;
using MongoDB.Driver;
using solDocs.Interfaces;
using solDocs.Dtos.Tenant;

namespace solDocs.Data
{
    public static class MongoDbInitializer
    {
        public static async Task ConfigureIndexesAsync(IMongoDatabase database)
        {
            var usersCollection = database.GetCollection<UserModel>("Users");
            var emailIndexKeys = Builders<UserModel>.IndexKeys.Ascending(user => user.Email);
            var emailIndexModel = new CreateIndexModel<UserModel>(emailIndexKeys, new CreateIndexOptions { Unique = true });
            var usernameIndexKeys = Builders<UserModel>.IndexKeys.Ascending(user => user.Username);
            var usernameIndexModel = new CreateIndexModel<UserModel>(usernameIndexKeys, new CreateIndexOptions { Unique = true });
            await usersCollection.Indexes.CreateManyAsync(new[] { emailIndexModel, usernameIndexModel });
            
            
            var imageCollection = database.GetCollection<MediaAssetModel>("MediaAssets");
            var imageIndexKeys = Builders<MediaAssetModel>.IndexKeys.Ascending(mediaAsset => mediaAsset.FileHash);
            var imageIndexModel = new CreateIndexModel<MediaAssetModel>(imageIndexKeys, new CreateIndexOptions { Unique = true });
            await imageCollection.Indexes.CreateManyAsync(new[] { imageIndexModel });
            
            var topicsCollection = database.GetCollection<TopicModel>("Topics");
            var topicNameIndexKeys = Builders<TopicModel>.IndexKeys.Ascending(topic => topic.Name);
            var topicNameIndexModel = new CreateIndexModel<TopicModel>(topicNameIndexKeys);
            await topicsCollection.Indexes.CreateOneAsync(topicNameIndexModel);


            var articlesCollection = database.GetCollection<ArticleModel>("Articles");
            var articleTextIndexKeys = Builders<ArticleModel>.IndexKeys.Combine(
                Builders<ArticleModel>.IndexKeys.Text(article => article.Title),
                Builders<ArticleModel>.IndexKeys.Text(article => article.Content)
            );
            
            var articleTextIndexOptions = new CreateIndexOptions { DefaultLanguage = "pt" };
            
            var articleTextIndexModel = new CreateIndexModel<ArticleModel>(articleTextIndexKeys, articleTextIndexOptions);
            await articlesCollection.Indexes.CreateOneAsync(articleTextIndexModel);
        }
        
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();

            var adminEmail = configuration["AdminUserSeed:Email"];
            if (await userService.GetUserByEmailAsync(adminEmail) != null)
            {
                Console.WriteLine("Admin user already exists. Skipping seed.");
                return;
            }
            
            const string defaultTenantSlug = "default";
            var defaultTenant = await tenantService.GetBySlugAsync(defaultTenantSlug);
            
            if (defaultTenant == null)
            {
                Console.WriteLine("Default tenant not found. Seeding...");
                var tenantDto = new CreateTenantDto
                {
                    Nome = "Default Tenant",
                    Slug = defaultTenantSlug,
                    Email = adminEmail, 
                    VencimentoDaLicenca = DateTime.UtcNow.AddYears(10),
                    PlanoId = "premium",
                    LimiteUsuarios = 999,
                    LimiteArmazenamento = long.MaxValue
                };
                defaultTenant = await tenantService.CreateAsync(tenantDto);
                Console.WriteLine("Default tenant created successfully.");
            }
            
            var adminUsername = configuration["AdminUserSeed:Username"];
            var adminPassword = configuration["AdminUserSeed:Password"];
            
            var adminUser = new UserModel
            {
                Email = adminEmail,
                Username = adminUsername,
                Roles = new List<string> { "admin", "super_admin" },
                TenantId = defaultTenant.Id
            };

            await userService.CreateUserAsync(adminUser, adminPassword);
        }
    }
}