using System.Net.Sockets;

public class PositionManager
{
    private Dictionary<int, PlayerData> _playerData;
    private UdpClient _udpClient;

    public PositionManager()
    {
        _playerData = new Dictionary<int, PlayerData>();
        _udpClient = new UdpClient(); // Initialize UdpClient for sending messages
    }

    public void AddOrUpdatePlayer(int playerId, PlayerData data)
    {
        _playerData[playerId] = data;
    }

    public IEnumerable<KeyValuePair<int, PlayerData>> GetAllPlayers()
    {
        return _playerData;
    }

    public async Task SendMessageToPlayer(int playerId, byte[] message)
    {
        if (_playerData.TryGetValue(playerId, out PlayerData playerData))
        {
            await _udpClient.SendAsync(message, message.Length, playerData.EndPoint);
        }
    }

    public bool TryGetPlayerData(int playerId, out PlayerData data)
    {
        return _playerData.TryGetValue(playerId, out data);
    }

    public void RemovePlayer(int playerId)
    {
        _playerData.Remove(playerId);
    }

    public void SendMessage(byte[] message)
    {
    }
}
