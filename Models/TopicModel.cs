using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace solDocs.Models
{
    public class TopicModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonRequired]
        [BsonElement("tenantId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TenantId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("parentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }
        
        [BsonElement("deletedAt")]
        [BsonIgnoreIfNull]
        public DateTime? DeletedAt { get; set; } = null;
    }
}