using dotenv.net;

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
                // These configurations to set up database connections, server settings, etc.
                var configLoader = new LoadServerConfiguration();
                var dbConfig = configLoader.DbConfig;
                var serverConfig = configLoader.ServerConfig;

                var dbInterface = new DbInterface(dbConfig.ConnectionString, dbConfig.DatabaseName);
                Console.WriteLine("Database connection established.");

                var playerManager = new PlayerManager(dbInterface);
                Console.WriteLine($"Player manager initialized.");

                var socketListener = new SocketListener(serverConfig.SocketListenerPort, playerManager);
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