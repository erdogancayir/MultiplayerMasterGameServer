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
                DotEnv.Load();
                Console.WriteLine("Starting Master Server...");

                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var serverConfig = serviceProvider.GetService<ServerConfig>();
                if (serverConfig == null)
                {
                    throw new InvalidOperationException("Server configuration could not be loaded.");
                }
                var socketListener = new SocketListener(serverConfig.SocketListenerPort, serviceProvider);
                Console.WriteLine($"Listening for connections on port {serverConfig.SocketListenerPort}...");
                socketListener.Start();

                // Additional initializations (Authentication, GameServerManager, etc.)
                // Keep the server running
                KeepServerRunning();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting the server: {ex.Message}");
            }
        }

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
            
            // Register other services
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<PlayerManager>();
            services.AddSingleton<Matchmaker>();
            // ...other service registrations...
        }


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