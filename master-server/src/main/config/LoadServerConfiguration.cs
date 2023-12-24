using MongoDB.Bson.IO;
using Newtonsoft.Json;

public class LoadServerConfiguration
{
    public DatabaseConfig DbConfig { get; private set; }
    public ServerConfig ServerConfig { get; private set; }

    public GameServerManager GameServerManager { get; private set; }

    public LoadServerConfiguration()
    {
        DbConfig = LoadDatabaseConfig();
        ServerConfig = LoadServerConfig();
        GameServerManager = LoadGameServerConfig();
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
        // Load server configuration
        // Placeholder example:
        return new ServerConfig
        {
            MaxPlayersPerLobby = 10,
            MaxLobbies = 5,
            SocketListenerPort = 8080,
            JwtSecretKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            // ...other settings
        };
    }

    private GameServerManager LoadGameServerConfig()
    {
        string gameServerConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "src/main/config", "gameServerConfig.json");
        string json = File.ReadAllText(gameServerConfigPath);
        var gameServers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GameServer>>(json);
        return new GameServerManager { GameServers = gameServers ?? new List<GameServer>() };
    }
}
