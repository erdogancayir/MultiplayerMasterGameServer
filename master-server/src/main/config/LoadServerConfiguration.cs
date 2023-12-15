public class LoadServerConfiguration
{
    public DatabaseConfig DbConfig { get; private set; }
    public ServerConfig ServerConfig { get; private set; }

    public LoadServerConfiguration()
    {
        // Load configurations here. This can be from a file, environment variables, etc.
        // Example:
        // DbConfig = LoadDatabaseConfig();
        // ServerConfig = LoadServerConfig();
    }

    // Methods to load individual configurations...
}
