using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionID { get; set; }

    public string PlayerID { get; set; }
    public string Token { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; } // Nullable for ongoing sessions
    public bool IsActive { get; set; }
}
