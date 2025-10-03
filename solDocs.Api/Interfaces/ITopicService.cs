using solDocs.Models;

namespace solDocs.Interfaces
{
    public interface ITopicService
    {
        Task<IEnumerable<TopicModel>> GetAllTopicsAsync(string tenantId, string type);
        Task<TopicModel?> GetTopicByIdAsync(string id, string tenantId);
        Task<TopicModel> CreateTopicAsync(TopicModel topic);
        Task<bool> DeleteTopicAsync(string id, string tenantId);
        Task<object> GetTopicTreeAsync(string tenantId, string type, bool isAuthenticated);
        Task<TopicModel?> UpdateTopicAsync(string id, string tenantId, TopicModel updatedTopic);
        Task<TopicModel?> UpdateTopicTypeAsync(string id, string tenantId, string newType);
    }
}