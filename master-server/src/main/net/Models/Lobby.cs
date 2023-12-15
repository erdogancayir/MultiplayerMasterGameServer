using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string LobbyID { get; set; }

    public List<string> Players { get; set; } // List of PlayerIDs
    public string Status { get; set; } // e.g., Waiting, Full, InGame
    public string GameType { get; set; }
    public DateTime CreationTime { get; set; }
    public int MaxPlayers { get; set; }
}
