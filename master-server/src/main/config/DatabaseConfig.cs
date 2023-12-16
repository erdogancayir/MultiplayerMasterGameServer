public class DatabaseConfig
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }

    public DatabaseConfig(string connectionString, string databaseName)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
    }
    // Additional settings for performance tuning, connection pooling, etc.
}
