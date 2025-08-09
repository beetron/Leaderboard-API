using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Leaderboard_API.Models
{
    public class Record
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("PlayerName")]
        public string PlayerName { get; set; } = string.Empty;

        [BsonElement("PlayerScore")]
        public int PlayerScore { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}