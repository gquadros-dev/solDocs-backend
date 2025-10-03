using solDocs.Models;

namespace solDocs.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleModel>> GetAllArticlesAsync(string tenantId); 
        Task<ArticleModel?> GetArticleByIdAsync(string id, string tenantId); 
        Task<IEnumerable<ArticleModel>> GetArticlesByTopicIdAsync(string topicId, string tenantId);
        Task<ArticleModel> CreateArticleAsync(ArticleModel article);
        Task<ArticleModel?> UpdateArticleAsync(string id, string tenantId, ArticleModel article); 
        Task<bool> DeleteArticleAsync(string id, string tenantId);
    }
}