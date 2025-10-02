using MongoDB.Driver;
using MongoDB.Bson;
using solDocs.Interfaces;
using solDocs.Models;
using solDocs.Dtos;

namespace solDocs.Services
{
    public class TopicService : ITopicService
    {
        private readonly IMongoCollection<TopicModel> _topicsCollection;
        private FilterDefinition<TopicModel> ActiveTopicsFilter => 
            Builders<TopicModel>.Filter.Eq(t => t.DeletedAt, null);

        public TopicService(IMongoDatabase database)
        {
            _topicsCollection = database.GetCollection<TopicModel>("Topics");
        }

        public async Task<IEnumerable<TopicModel>> GetAllTopicsAsync(string tenantId, string type)
        {
            var builder = Builders<TopicModel>.Filter;
            var filter = ActiveTopicsFilter & builder.Eq(t => t.TenantId, tenantId);
            
            if (!string.IsNullOrEmpty(type))
            {
                filter &= builder.Eq(t => t.Type, type);
            }

            return await _topicsCollection.Find(filter).ToListAsync();
        }

        public async Task<TopicModel?> GetTopicByIdAsync(string id, string tenantId)
        {
            var filter = ActiveTopicsFilter & 
                         Builders<TopicModel>.Filter.Eq(t => t.Id, id) &
                         Builders<TopicModel>.Filter.Eq(t => t.TenantId, tenantId);
            return await _topicsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<TopicModel> CreateTopicAsync(TopicModel topic)
        {
            topic.DeletedAt = null;
            await _topicsCollection.InsertOneAsync(topic);
            return topic;
        }

        public async Task<bool> DeleteTopicAsync(string id, string tenantId)
        {
            var filter = ActiveTopicsFilter & 
                         Builders<TopicModel>.Filter.Eq(t => t.Id, id) &
                         Builders<TopicModel>.Filter.Eq(t => t.TenantId, tenantId);
            var update = Builders<TopicModel>.Update.Set(t => t.DeletedAt, DateTime.UtcNow);
            var result = await _topicsCollection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<TopicModel?> UpdateTopicAsync(string id, string tenantId, TopicModel topicWithNewValues)
        {
            var filter = ActiveTopicsFilter & 
                         Builders<TopicModel>.Filter.Eq(t => t.Id, id) &
                         Builders<TopicModel>.Filter.Eq(t => t.TenantId, tenantId);
            var updateDefinition = Builders<TopicModel>.Update
                .Set(t => t.Name, topicWithNewValues.Name)
                .Set(t => t.Type, topicWithNewValues.Type)
                .Set(t => t.ParentId, topicWithNewValues.ParentId)
                .Set(t => t.Order, topicWithNewValues.Order);
            var options = new FindOneAndUpdateOptions<TopicModel> { ReturnDocument = ReturnDocument.After };
            return await _topicsCollection.FindOneAndUpdateAsync(filter, updateDefinition, options);
        }

        public async Task<object> GetTopicTreeAsync(string tenantId, string type, bool isAuthenticated)
        {
            var matchFilter = new BsonDocument
            {
                { "deletedAt", BsonNull.Value },
                { "tenantId", new BsonObjectId(ObjectId.Parse(tenantId)) }
            };

            if (isAuthenticated)
            {
                matchFilter.Add("type", type);
            }
            else
            {
                matchFilter.Add("type", "public");
            }
            
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", matchFilter),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Articles" },
                    { "localField", "_id" },
                    { "foreignField", "topicId" },
                    { "as", "articles" },
                    { "pipeline", new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument 
                            { 
                                { "deletedAt", BsonNull.Value },
                                { "tenantId", new BsonObjectId(ObjectId.Parse(tenantId)) }
                            }), 
                            new BsonDocument("$project", new BsonDocument { { "_id", 1 }, { "title", 1 } })
                        }
                    }
                })
            };

            var allTopicsWithArticles = await _topicsCollection.Aggregate<TopicTreeDto>(pipeline).ToListAsync();
            var topicMap = allTopicsWithArticles.ToDictionary(t => t.Id);
            var rootTopics = new List<TopicTreeDto>();

            foreach (var topic in allTopicsWithArticles)
            {
                if (topic.ParentId != null && topicMap.TryGetValue(topic.ParentId, out var parent))
                {
                    parent.Children.Add(topic);
                }
                else
                {
                    rootTopics.Add(topic);
                }
            }
    
            return rootTopics.OrderBy(t => t.Order).ToList();
        }
        
        public async Task<TopicModel?> UpdateTopicTypeAsync(string id, string tenantId, string newType)
        {
            var filter = ActiveTopicsFilter & 
                         Builders<TopicModel>.Filter.Eq(t => t.Id, id) &
                         Builders<TopicModel>.Filter.Eq(t => t.TenantId, tenantId);
            var updateDefinition = Builders<TopicModel>.Update.Set(t => t.Type, newType);
            var options = new FindOneAndUpdateOptions<TopicModel> { ReturnDocument = ReturnDocument.After };
            return await _topicsCollection.FindOneAndUpdateAsync(filter, updateDefinition, options);
        }
    }
}
