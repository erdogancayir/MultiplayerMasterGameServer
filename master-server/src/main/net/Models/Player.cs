using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Player
{
    [BsonId] // This attribute marks the property as the primary key (_id field in MongoDB)
    [BsonRepresentation(BsonType.ObjectId)] // This represents the MongoDB ObjectId type
    public ObjectId Id { get; set; }

    [BsonElement("username")] // Maps the property to the "username" field in the MongoDB document
    public string Username { get; set; }

    // ... Other properties with their respective BsonElement attributes ...
}
