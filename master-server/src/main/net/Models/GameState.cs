using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class GameState
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string StateID { get; set; }

    public string GameID { get; set; }
    // PlayersState could be a complex object representing the current state of each player
    public string PlayersState { get; set; } 
    public DateTime LastUpdateTime { get; set; }
}
