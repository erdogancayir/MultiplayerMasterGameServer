using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string LogID { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., Error, Info, Warning
    public string Message { get; set; } = string.Empty;
    public int? PlayerID { get; set; }
    public string? ServerID { get; set; }
}
