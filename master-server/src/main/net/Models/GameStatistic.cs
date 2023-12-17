using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GameStatistic
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string StatID { get; set; }

    public string PlayerID { get; set; }
    public string GameID { get; set; }
}
