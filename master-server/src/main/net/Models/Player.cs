using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int PlayerID { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime LastLoginDate { get; set; }

}

public class Counter
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
    public int SeqValue { get; set; }
}