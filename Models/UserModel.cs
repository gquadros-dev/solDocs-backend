using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace solDocs.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonRequired]
        [BsonElement("tenantId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TenantId { get; set; }
        
        [BsonRequired]
        [BsonElement("username")]
        public string Username { get; set; }

        [BsonRequired]        
        [BsonElement("email")]
        public string Email { get; set; }

        [BsonRequired]
        [BsonElement("hashed_password")]
        public string HashedPassword { get; set; }
        
        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; }
    }   
}
