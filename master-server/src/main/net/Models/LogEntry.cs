using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string LogID { get; set; }

    public DateTime Timestamp { get; set; }
    public string Type { get; set; } // e.g., Error, Info, Warning
    public string Message { get; set; }
    public string PlayerID { get; set; } // Optional
    public string ServerID { get; set; } // Optional
}
