namespace MasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Master Server...");
            // These configurations to set up database connections, server settings, etc.
            var configLoader = new LoadServerConfiguration();
            var dbConfig = configLoader.DbConfig;
            var serverConfig = configLoader.ServerConfig;
            
            // Initialize Socket Listener
            var socketListener = new SocketListener(serverConfig.SocketListenerPort);
            Console.WriteLine($"Listening for connections on port {serverConfig.SocketListenerPort}...");
            socketListener.Start();

            // Additional initializations (Authentication, GameServerManager, etc.)

            // Keep the server running
            KeepServerRunning();
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