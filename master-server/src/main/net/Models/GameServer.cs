using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GameServer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ServerID { get; set; }

    public string IP { get; set; }
    public int Port { get; set; }
    public string Status { get; set; } // e.g., Active, Maintenance, Offline
    public int CurrentLoad { get; set; }
    public int MaxCapacity { get; set; }
    public string LastHeartbeatTime { get; set; }
    public string? Region { get; set; }
    public DateTime CreationTime { get; set; }
}
