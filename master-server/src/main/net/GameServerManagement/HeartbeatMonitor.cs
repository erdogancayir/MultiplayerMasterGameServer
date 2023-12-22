public class HeartbeatMonitor
{
    private readonly List<GameServer> _gameServers;
    private Timer _timer;
    private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromMinutes(5);

    public HeartbeatMonitor(List<GameServer> gameServers)
    {
        _gameServers = gameServers;
    }

    public void StartMonitoring()
    {
        _timer = new Timer(CheckServerStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Her 1 dakikada bir kontrol
    }

    private void CheckServerStatus(object state)
    {
        foreach (var server in _gameServers)
        {
            if (!IsServerAlive(server))
            {
                HandleServerFailure(server);
            }
        }
    }

    private bool IsServerAlive(GameServer server)
    {
        if (DateTime.TryParse(server.LastHeartbeatTime, out var lastHeartbeat))
        {
            return DateTime.UtcNow - lastHeartbeat < _heartbeatTimeout;
        }
        return false;
    }

    private void HandleServerFailure(GameServer server)
    {
        server.Status = "Offline";
        // Sunucu başarısız olduğunda yapılacak işlemler...
    }

    public void StopMonitoring()
    {
        _timer?.Dispose();
    }
}
