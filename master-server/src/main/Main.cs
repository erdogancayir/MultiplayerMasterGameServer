using dotenv.net;
using Microsoft.Extensions.DependencyInjection;


namespace MasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool serverRunning = true;

            while (serverRunning)
            {
                try
                {
                    StartServer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting the server: {ex.Message}");
                    Console.WriteLine("Attempting to restart server...");
                    Thread.Sleep(5000);
                }
            }
        }

        private static void StartServer()
        {
            // Load environment variables from .env file
            DotEnv.Load();
            Console.WriteLine("Starting Master Server...");

            // Set up dependency injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Initialize counters
            InitializeCounters(serviceProvider).GetAwaiter().GetResult();

            var serverConfig = serviceProvider.GetService<ServerConfig>();
            if (serverConfig == null)
                throw new InvalidOperationException("Server configuration could not be loaded.");

            // Initialize and start the socket listener
            var socketListener = new SocketListener(serverConfig.SocketListenerPort, serviceProvider);
            socketListener.Start();

            // Keep the server running
            KeepServerRunning();
        }
        /* public static void Main(string[] args)
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

                // Initialize counters
                InitializeCounters(serviceProvider).GetAwaiter().GetResult();

                var serverConfig = serviceProvider.GetService<ServerConfig>();
                if (serverConfig == null)
                    throw new InvalidOperationException("Server configuration could not be loaded.");

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
        } */

        #region Dependencies Setup

        /// <summary>
        /// Configures the dependency injection container.
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureServices(IServiceCollection services)
        {
            LoadConfigurations(services);
            RegisterDatabaseServices(services);
            RegisterCoreServices(services);
            RegisterDomainServices(services);
            RegisterUtilityServices(services);
        }

        private static void LoadConfigurations(IServiceCollection services)
        {
            var configLoader = new LoadServerConfiguration();
            services.AddSingleton(configLoader.DbConfig);
            services.AddSingleton(configLoader.ServerConfig);
            services.AddSingleton(configLoader.GameServerManager);
        }

        private static void RegisterDatabaseServices(IServiceCollection services)
        {
            var configLoader = new LoadServerConfiguration();
            var dbConnectionString = configLoader.DbConfig.ConnectionString;
            var dbName = configLoader.DbConfig.DatabaseName;
            services.AddSingleton<DbInterface>(provider => new DbInterface(dbConnectionString, dbName));
        }

        private static void RegisterCoreServices(IServiceCollection services)
        {
            var randomSecretKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            services.AddSingleton<LogManager>();
            services.AddSingleton<TokenManager>(provider =>
                new TokenManager(provider.GetRequiredService<ServerConfig>().JwtSecretKey ?? randomSecretKey));
        }

        private static void RegisterDomainServices(IServiceCollection services)
        {
            services.AddSingleton<PlayerManager>();
            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<LobbyManager>();
            services.AddSingleton<Matchmaker>();
            services.AddSingleton<GameManager>();
            services.AddSingleton<LeaderboardManager>();
        }

        private static void RegisterUtilityServices(IServiceCollection services)
        {
            services.AddSingleton<AuthService>();
            services.AddSingleton<HeartbeatManager>(provider =>
            {
                var dbInterface = provider.GetRequiredService<DbInterface>();
                var gameServerManager = provider.GetRequiredService<GameServerManager>();
                var gameServers = gameServerManager.GameServers;
                return new HeartbeatManager(dbInterface, gameServers);
            });
        }
        #endregion

        private static async Task InitializeCounters(IServiceProvider serviceProvider)
        {
            var playerManager = serviceProvider.GetRequiredService<PlayerManager>();
            await playerManager.InitializeCounterAsync();
        }

        /// <summary>
        /// Keeps the server running, handling logic for a graceful shutdown.
        /// </summary>
        private static void KeepServerRunning()
        {
            while (true) {}
        }
    }

}