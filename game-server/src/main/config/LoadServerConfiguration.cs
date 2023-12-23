public class LoadServerConfiguration
{
    public DatabaseConfig DbConfig { get; private set; }
    public ServerConfig ServerConfig { get; private set; }

    public LoadServerConfiguration()
    {
        DbConfig = LoadDatabaseConfig();
        ServerConfig = LoadServerConfig();
    }

    private DatabaseConfig LoadDatabaseConfig()
    {
        // Load database configuration from a file, environment variable, etc.
        string? connectionString = Environment.GetEnvironmentVariable("PANTEON_DB_CONNECTION_STRING");
        string? databaseName = Environment.GetEnvironmentVariable("PANTEON_DB_NAME");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("Database configuration not found.");
        }
        return new DatabaseConfig(connectionString, databaseName);
    }

    private ServerConfig LoadServerConfig()
    {
        return new ServerConfig
        {
            SocketTcpListenerPort = 8081,
            SocketUdpListenerPort = 8082,
            MasterServerTcpPort = 8080,
            MasterServerIp = "127.0.0.1",
            JwtSecretKey = "secret-key"
        };
    }
}
