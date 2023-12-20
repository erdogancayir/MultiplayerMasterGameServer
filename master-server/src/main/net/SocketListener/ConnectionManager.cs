using System.Net.Sockets;

public class ConnectionManager
{
    private readonly Dictionary<string, TcpClient> _connections = new Dictionary<string, TcpClient>();

    public void AddConnection(string playerId, TcpClient client)
    {
        _connections[playerId] = client;
    }

    public TcpClient? GetConnection(string playerId)
    {
        //Console.WriteLine("opssss");
        _connections.TryGetValue(playerId, out TcpClient? client);
        return client;
    }

    public void UpdateConnectionId(string oldConnectionId, string newPlayerId)
    {
        if (_connections.TryGetValue(oldConnectionId, out TcpClient? client))
        {
            _connections.Remove(oldConnectionId);
            _connections[newPlayerId] = client;
        }
    }

    public void RemoveConnection(TcpClient client)
    {
        var playerId = _connections.FirstOrDefault(x => x.Value == client).Key;
        if (playerId != null)
        {
            _connections.Remove(playerId);
        }
    }

    public void RemoveConnectionById(string playerId)
    {
        if (_connections.ContainsKey(playerId))
        {
            _connections.Remove(playerId);
        }
    }
}
