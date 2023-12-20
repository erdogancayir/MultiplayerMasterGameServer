using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class LeaderboardEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LeaderboardEntryID { get; set; }

    public string? PlayerID { get; set; }
    public int TotalPoints { get; set; } // Total points accumulated
}
