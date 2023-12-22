using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LobbyID { get; set; }

    public List<int>? Players { get; set; } // List of PlayerIDs
    public LobbyStatus? Status { get; set; } // Now using the LobbyStatus enum
    public DateTime CreationTime { get; set; }
    public int MaxPlayers { get; set; }
    public enum LobbyStatus
    {
        Waiting,
        Full,
        InGame,
        Closed,
        DefaultStatus
    }
}
