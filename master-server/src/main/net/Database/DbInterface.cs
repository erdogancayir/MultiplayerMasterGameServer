using MongoDB.Driver;

public class DbInterface
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the DbInterface class.
    /// </summary>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <exception cref="InvalidOperationException">Thrown if the database connection fails.</exception>
    public DbInterface(string connectionString, string databaseName)
    {
        try
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);

            // Perform a simple read operation to validate the connection
            _database.ListCollectionNames().FirstOrDefault();
        }
        catch (MongoException ex)
        {
            throw new InvalidOperationException("Failed to establish database connection.", ex);
        }
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
