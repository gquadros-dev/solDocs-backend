using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace solDocs.Models
{
    public class MediaAssetModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonRequired]
        [BsonElement("tenantId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TenantId { get; set; }

        [BsonElement("fileHash")] 
        public string FileHash { get; set; } = null!;

        [BsonElement("originalFileName")]
        public string OriginalFileName { get; set; } = null!;

        [BsonElement("fileSize")]
        public long FileSize { get; set; }

        [BsonElement("mimeType")]
        public string MimeType { get; set; } = null!;

        [BsonElement("publicUrl")]
        public string PublicUrl { get; set; } = null!;
        
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}