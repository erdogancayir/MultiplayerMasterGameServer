using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Game
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? GameID { get; set; }
    public int PlayerID { get; set; }
    public string? LobbyID { get; set; }
    public DateTime EndTime { get; set; }
    public GameStatus Status { get; set; }

    public enum GameStatus
    {
        InProgress,
        Completed
    }
}
