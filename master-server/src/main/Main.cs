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

                // Retrieve server configuration
                var serverConfig = serviceProvider.GetService<ServerConfig>();
                if (serverConfig == null)
                {
                    throw new InvalidOperationException("Server configuration could not be loaded.");
                }

                // Initialize and start the socket listener
                var socketListener = new SocketListener(serverConfig.SocketListenerPort, serviceProvider);
                Console.WriteLine($"Listening for connections on port {serverConfig.SocketListenerPort}...");
                socketListener.Start();

                // Keep the server running
                KeepServerRunning();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting the server: {ex.Message}");
            }
        }

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

            // Register PlayerManager with LogManager dependency
            services.AddSingleton<PlayerManager>(provider => 
                new PlayerManager(provider.GetRequiredService<DbInterface>(), provider.GetRequiredService<LogManager>()));

            // Register AuthService with LogManager dependency
            services.AddSingleton<IAuthService, AuthService>(provider =>
                new AuthService(provider.GetRequiredService<PlayerManager>(), provider.GetRequiredService<LogManager>()));

            services.AddSingleton<Matchmaker>();
            // ...other service registrations...
        }


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