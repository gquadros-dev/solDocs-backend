using solDocs.Interfaces;
using solDocs.Models;
using MongoDB.Driver;

namespace solDocs.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IMongoCollection<ArticleModel> _articlesCollection;
        private FilterDefinition<ArticleModel> ActiveArticlesFilter => 
            Builders<ArticleModel>.Filter.Eq(a => a.DeletedAt, null);
        
        public ArticleService(IMongoDatabase database)
        {
            _articlesCollection = database.GetCollection<ArticleModel>("Articles");
        }

        public async Task<ArticleModel> CreateArticleAsync(ArticleModel article)
        {
            article.CreatedAt = DateTime.UtcNow;
            article.DeletedAt = null; 
            await _articlesCollection.InsertOneAsync(article);
            return article;
        }

        public async Task<bool> DeleteArticleAsync(string id, string tenantId)
        {
            var filter = ActiveArticlesFilter & 
                         Builders<ArticleModel>.Filter.Eq(a => a.Id, id) &
                         Builders<ArticleModel>.Filter.Eq(a => a.TenantId, tenantId);
            
            var update = Builders<ArticleModel>.Update.Set(a => a.DeletedAt, DateTime.UtcNow);
            var result = await _articlesCollection.UpdateOneAsync(filter, update);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<ArticleModel>> GetAllArticlesAsync(string tenantId)
        {
            var filter = ActiveArticlesFilter & Builders<ArticleModel>.Filter.Eq(a => a.TenantId, tenantId);
            return await _articlesCollection.Find(filter).ToListAsync();
        }

        public async Task<ArticleModel?> GetArticleByIdAsync(string id, string tenantId)
        {
            var filter = ActiveArticlesFilter & 
                         Builders<ArticleModel>.Filter.Eq(a => a.Id, id) &
                         Builders<ArticleModel>.Filter.Eq(a => a.TenantId, tenantId);
            return await _articlesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ArticleModel>> GetArticlesByTopicIdAsync(string topicId, string tenantId)
        {
            var filter = ActiveArticlesFilter & 
                         Builders<ArticleModel>.Filter.Eq(a => a.TopicId, topicId) &
                         Builders<ArticleModel>.Filter.Eq(a => a.TenantId, tenantId);
            return await _articlesCollection.Find(filter).ToListAsync();
        }

        public async Task<ArticleModel?> UpdateArticleAsync(string id, string tenantId, ArticleModel article)
        {
            var filter = ActiveArticlesFilter & 
                         Builders<ArticleModel>.Filter.Eq(a => a.Id, id) &
                         Builders<ArticleModel>.Filter.Eq(a => a.TenantId, tenantId);

            var updateDefinition = Builders<ArticleModel>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Content, article.Content);
            
            var options = new FindOneAndUpdateOptions<ArticleModel>
            {
                ReturnDocument = ReturnDocument.After
            };
            
            return await _articlesCollection.FindOneAndUpdateAsync(filter, updateDefinition, options);
        }
    }
}