using System.Net.Sockets;

public class PositionManager
{
    private Dictionary<int, PlayerData> _playerData;
    private Dictionary<string, List<int>> _lobbyPlayers; // int : Lobby id New: PlayerIds -> List of PlayerIds
    private UdpClient _udpClient;

    public PositionManager()
    {
        _playerData = new Dictionary<int, PlayerData>();
        _lobbyPlayers = new Dictionary<string, List<int>>();
        _udpClient = new UdpClient();
    }

    public void AddOrUpdatePlayer(int playerId, PlayerData data)
    {
        _playerData[playerId] = data;

        if (!_lobbyPlayers.ContainsKey(data.LobbyId))
        {
            _lobbyPlayers[data.LobbyId] = new List<int>();
        }
        if (!_lobbyPlayers[data.LobbyId].Contains(playerId))
        {
            _lobbyPlayers[data.LobbyId].Add(playerId);
        }
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

    public async Task SendMessageToLobby(int ownPlayerId, string lobbyId, byte[] message)
    {
        if (_lobbyPlayers.TryGetValue(lobbyId, out List<int> playerIds))
        {
            foreach (var playerId in playerIds)
            {
                if (ownPlayerId != playerId && _playerData.TryGetValue(playerId, out PlayerData playerData))
                {
                    await _udpClient.SendAsync(message, message.Length, playerData.EndPoint);
                }
            }
        }
    }

    public bool TryGetPlayerData(int playerId, out PlayerData data)
    {
        return _playerData.TryGetValue(playerId, out data);
    }

    public void RemovePlayer(int playerId)
    {
        if (_playerData.TryGetValue(playerId, out PlayerData data))
        {
            _lobbyPlayers[data.LobbyId].Remove(playerId);
            _playerData.Remove(playerId);
        }
    }
}