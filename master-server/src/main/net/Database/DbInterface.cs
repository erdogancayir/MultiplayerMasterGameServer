using MongoDB.Driver;

public class DbInterface
{
    private readonly IMongoDatabase _database;

    public DbInterface(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    // ... Additional CRUD operations go here ...
}
