using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Matchmaking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MatchID { get; set; }

    public List<int>? Players { get; set; } // List of PlayerIDs
    public string? LobbyID { get; set; }
    public DateTime StartTime { get; set; }
    public string? Status { get; set; }
}