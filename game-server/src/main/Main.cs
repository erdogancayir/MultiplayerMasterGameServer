using System.Net.Sockets;
using Amazon.Runtime.Internal;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DotEnv.Load();
                Console.WriteLine("Starting Game Server...");
                // Set up dependency injection
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // Build the service provider
                var serviceProvider = serviceCollection.BuildServiceProvider();
                 // Retrieve server configuration
                var serverConfig = serviceProvider.GetService<ServerConfig>() ?? throw new InvalidOperationException("Server configuration not found.");
                // Start the socket listener
                var socketListener = new SocketListener(serverConfig.SocketTcpListenerPort, serverConfig.SocketUdpListenerPort, serviceProvider);
                socketListener.Start();
                // Keep the server running
                KeepServerRunning();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
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
            services.AddSingleton<ConnectionMasterServer>(provider =>
            {
                var tcpClient = new TcpClient(configLoader.ServerConfig.MasterServerIp, configLoader.ServerConfig.MasterServerTcpPort);
                return new ConnectionMasterServer(tcpClient);
            });
            services.AddSingleton<PositionManager>();
            services.AddSingleton<TcpConnectionManager>();
            services.AddSingleton<UdpConnectionManager>();
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

