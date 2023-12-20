using dotenv.net;
using Microsoft.Extensions.DependencyInjection;


namespace MasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try 
            {
                // Load environment variables from .env file
                DotEnv.Load();
                Console.WriteLine("Starting Master Server...");

                // Set up dependency injection
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // Build the service provider
                var serviceProvider = serviceCollection.BuildServiceProvider();

                var serverConfig = serviceProvider.GetService<ServerConfig>();
                if (serverConfig == null)
                {
                    throw new InvalidOperationException("Server configuration could not be loaded.");
                }
                // Initialize and start the socket listener
                var socketListener = new SocketListener(serverConfig.SocketListenerPort, serviceProvider);
                socketListener.Start();

                // Keep the server running
                KeepServerRunning();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting the server: {ex.Message}");
            }
        }

        #region Dependencies Setup
        /// <summary>
        /// Configures services for dependency injection.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            // Load configurations
            var configLoader = new LoadServerConfiguration();
            services.AddSingleton(configLoader.DbConfig);
            services.AddSingleton(configLoader.ServerConfig);
            // Assuming DbConfig has properties for the connection string and database name
            var dbConnectionString = configLoader.DbConfig.ConnectionString;
            var dbName = configLoader.DbConfig.DatabaseName;
            // Register DbInterface with necessary parameters
            services.AddSingleton<DbInterface>(provider => new DbInterface(dbConnectionString, dbName));
            // Register LogManager
            services.AddSingleton<LogManager>();
            // Register the implementation of ITokenStorage
            var randomKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            // Register TokenManager with its dependencies
            services.AddSingleton<TokenManager>(provider => 
                new TokenManager(configLoader.ServerConfig.JwtSecretKey ?? randomKey));
            // Register PlayerManager with LogManager, TokenManager, and DbInterface dependencies
            services.AddSingleton<PlayerManager>(provider => 
                new PlayerManager(
                    provider.GetRequiredService<DbInterface>(), 
                    provider.GetRequiredService<LogManager>(),
                    provider.GetRequiredService<TokenManager>()
                ));
            // Register ConnectionManager
            services.AddSingleton<ConnectionManager>();
            // Register AuthService with LogManager,... dependency
            services.AddSingleton<AuthService>(provider =>
                new AuthService(provider.GetRequiredService<PlayerManager>(), provider.GetRequiredService<LogManager>(),
                    provider.GetRequiredService<ConnectionManager>()));
            // Register LobbyManager with DbInterface dependency
            services.AddSingleton<LobbyManager>(provider =>
                new LobbyManager(provider.GetRequiredService<DbInterface>()));
            // Register Matchmaker with LobbyManager,... dependency
            services.AddSingleton<Matchmaker>(provider =>
                new Matchmaker(provider.GetRequiredService<LobbyManager>(),
                    provider.GetRequiredService<TokenManager>(),
                    provider.GetRequiredService<PlayerManager>(),
                    provider.GetRequiredService<ConnectionManager>()
                ));
            services.AddSingleton<GameManager>(provider =>
                new GameManager(provider.GetRequiredService<DbInterface>()));
            services.AddSingleton<GameStatisticsManager>(provider => 
                new GameStatisticsManager(provider.GetRequiredService<DbInterface>()));
            services.AddSingleton<LeaderboardManager>(provider =>
                new LeaderboardManager(provider.GetRequiredService<DbInterface>()));
        }
        #endregion

        /// <summary>
        /// Keeps the server running, handling logic for a graceful shutdown.
        /// </summary>
        private static void KeepServerRunning()
        {
            // Logic to keep the server running, handle graceful shutdown, etc.
            while (true)
            {
                // Server logic or waiting for termination signal
            }
        }
    }

}