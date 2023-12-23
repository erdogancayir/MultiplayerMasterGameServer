using System.Net.Sockets;
using MessagePack;

public class HeartbeatManager
{
    private readonly List<GameServer> _gameServers;
    private Timer? _timer;
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMinutes(1);
    private readonly DbInterface _dbInterface;

    public HeartbeatManager(DbInterface dbInterface, List<GameServer> gameServers)
    {
        _dbInterface = dbInterface;
        _gameServers = gameServers;
    }

    /// <summary>
    /// Starts sending heartbeats to all game servers.
    /// </summary>
    public void StartSendingHeartbeats()
    {
        _timer = new Timer(SendHeartbeatToAllServers, null, TimeSpan.Zero, _heartbeatInterval);
    }

    /// <summary>
    /// Sends a heartbeat to all game servers.
    /// </summary>
    /// <param name="state"></param>
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
            if (server == null || server.IP == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            using (var client = new TcpClient(server.IP, server.Port))
            {
                var heartbeatMessage = new HeartbeatMessage
                {
                    OperationTypeId = (int)OperationType.HeartbeatPing,
                    ServerID = server.ServerID,
                    Timestamp = DateTime.UtcNow
                };
                var data = MessagePackSerializer.Serialize(heartbeatMessage);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server {server.ServerID} is not responding: {ex.Message}");
        }
    }

    public void StopSendingHeartbeats()
    {
        _timer?.Dispose();
    }
}
