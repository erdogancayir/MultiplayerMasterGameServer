using System.Net.Sockets;

public class ConnectionManager
{
    private readonly Dictionary<int, TcpClient> _connections = new Dictionary<int, TcpClient>();

    public void AddConnection(int playerId, TcpClient client)
    {
        _connections[playerId] = client;
    }

    public TcpClient? GetConnection(int playerId)
    {
        //Console.WriteLine("opssss");
        _connections.TryGetValue(playerId, out TcpClient? client);
        return client;
    }

    public void UpdateConnectionId(int oldConnectionId, int newPlayerId)
    {
        if (_connections.TryGetValue(oldConnectionId, out TcpClient? client))
        {
            _connections.Remove(oldConnectionId);
            _connections[newPlayerId] = client;
        }
    }

    public void RemoveConnection(TcpClient client)
    {
        var connection = _connections.FirstOrDefault(x => x.Value == client);
        if (connection.Key != 0)
        {
            _connections.Remove(connection.Key);
        }
    }

    public void RemoveConnectionById(int playerId)
    {
        if (_connections.ContainsKey(playerId))
        {
            _connections.Remove(playerId);
        }
    }
}
