using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using solDocs.Dtos.Article;

namespace solDocs.Dtos.Topic
{
    public class TopicTreeDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!;
        
        [BsonElement("tenantId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TenantId { get; set; } = null!;

        [BsonElement("parentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }
        
        [BsonElement("deletedAt")]
        [BsonIgnoreIfNull]
        public DateTime? DeletedAt { get; set; } = null;
    
        [BsonElement("articles")]
        public List<ArticleInTopicDto> Articles { get; set; } = new List<ArticleInTopicDto>();

        public List<TopicTreeDto> Children { get; set; } = new List<TopicTreeDto>();
        public int Level { get; set; }
    }
}
