using System.Net.Sockets;

public class HeartbeatManager
{
    private readonly List<GameServer> _gameServers;
    private Timer _timer;
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMinutes(1);
    private readonly DbInterface _dbInterface;

    public HeartbeatManager(DbInterface dbInterface, List<GameServer> gameServers)
    {
        _dbInterface = dbInterface;
        _gameServers = gameServers;
        // Timer ve diğer başlangıç işlemleri...
    }

    public void StartSendingHeartbeats()
    {
        _timer = new Timer(SendHeartbeatToAllServers, null, TimeSpan.Zero, _heartbeatInterval);
    }

    private void SendHeartbeatToAllServers(object state)
    {
        foreach (var server in _gameServers)
        {
            SendHeartbeat(server);
            Console.WriteLine($"Heartbeat sent to server {server.ServerID}");
        }
    }

    private void SendHeartbeat(GameServer server)
    {
        try
        {
            using (var client = new TcpClient(server.IP, server.Port))
            {
                var buffer = System.Text.Encoding.ASCII.GetBytes("heartbeat");
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            // Sunucuyla iletişim kurulamadığında yapılacak işlemler
            Console.WriteLine($"Server {server.ServerID} is not responding: {ex.Message}");
        }
    }

    public void StopSendingHeartbeats()
    {
        _timer?.Dispose();
    }
}
