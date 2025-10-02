using MongoDB.Bson.Serialization.Attributes;

namespace solDocs.Dtos
{
    public class ArticleInTopicDto
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; } = null!;

        [BsonElement("title")]
        public string Title { get; set; } = null!;
    }
}