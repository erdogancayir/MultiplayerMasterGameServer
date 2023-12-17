using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? PlayerID { get; set; }

    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    // Additional profile data can be added here
}