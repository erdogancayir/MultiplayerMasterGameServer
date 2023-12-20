using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GameStatistic
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? GameStatisticsID { get; set; }

    public string? PlayerID { get; set; }
    public string? GameID { get; set; }
    public int Rank { get; set; }
    public int Points { get; set; } // Points awarded in the game
}
