using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Game
{
    public string? LobbyID { get; set; }
    public DateTime EndTime { get; set; }
    public GameStatus Status { get; set; }

    public enum GameStatus
    {
        InProgress,
        Completed
    }
}
