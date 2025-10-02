using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace solDocs.Models
{
    public class PasswordResetTokenModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("hashedCode")]
        public string HashedCode { get; set; } = null!;

        [BsonElement("expiresAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ExpiresAt { get; set; }
    }
}