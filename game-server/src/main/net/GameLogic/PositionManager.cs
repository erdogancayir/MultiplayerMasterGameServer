using System.Net.Sockets;

public class PositionManager
{
    private Dictionary<int, PlayerData> _playerData; // int : PlayerId New: PlayerData -> PlayerId to PlayerData
    private Dictionary<string, List<int>> _lobbyPlayers; // string : Lobby id New: PlayerIds -> List of PlayerIds
    private UdpClient _udpClient; // New: UdpClient

    public PositionManager()
    {
        _playerData = new Dictionary<int, PlayerData>();
        _lobbyPlayers = new Dictionary<string, List<int>>();
        _udpClient = new UdpClient();
    }

    /// <summary>
    /// Adds or updates a player in the position manager.
    /// If the player is new, it will be added to the lobby.
    /// If the player already exists, the lobby will be updated.
    /// </summary>
    public void AddOrUpdatePlayer(int playerId, PlayerData data)
    {
        _playerData[playerId] = data;
        if (data == null || string.IsNullOrEmpty(data.LobbyId))
        {
            return;
        }
        if (!_lobbyPlayers.ContainsKey(data.LobbyId))
        {
            _lobbyPlayers[data.LobbyId] = new List<int>();
        }
        if (!_lobbyPlayers[data.LobbyId].Contains(playerId))
        {
            _lobbyPlayers[data.LobbyId].Add(playerId);
        }
    }

    /// <summary>
    /// Returns all players in the position manager.
    /// </summary>
    public IEnumerable<KeyValuePair<int, PlayerData>> GetAllPlayers()
    {
        return _playerData;
    }

    /// <summary>
    /// Returns all players in the position manager that are in the same lobby as the given player.
    /// </summary>
    public async Task SendMessageToLobby(int ownPlayerId, string lobbyId, byte[] message)
    {
        if (_lobbyPlayers.TryGetValue(lobbyId, out List<int>? playerIds))
        {
            foreach (var playerId in playerIds)
            {
                if (ownPlayerId != playerId && _playerData.TryGetValue(playerId, out PlayerData? playerData))
                {
                    await _udpClient.SendAsync(message, message.Length, playerData.EndPoint);
                }
            }
        }
    }

    /// <summary>
    /// Returns the player data for the given player id.
    /// </summary>
    public bool TryGetPlayerData(int playerId, out PlayerData data)
    {
        return _playerData.TryGetValue(playerId, out data);
    }

    /// <summary>
    /// Removes the player from the position manager.
    /// </summary>
    public void RemovePlayer(int playerId)
    {
        if (_playerData.TryGetValue(playerId, out PlayerData? data))
        {
            if (data.LobbyId != null) 
            {
                _lobbyPlayers[data.LobbyId].Remove(playerId);
            }
            _playerData.Remove(playerId);
        }
    }

    /// <summary>
    /// Returns all players in the position manager that are in the same lobby as the given player.
    /// </summary>
    public List<int> GetPlayerIdsInLobby(string lobbyId)
    {
        if (_lobbyPlayers.TryGetValue(lobbyId, out List<int>? playerIds))
        {
            return playerIds;
        }
        return new List<int>();
    }

    /// <summary>
    /// Returns the lobby id for the given player id.
    /// </summary>
    public string? GetPlayerLobbyId(int playerId)
    {
        if (_playerData.TryGetValue(playerId, out PlayerData? data))
        {
            return data.LobbyId;
        }
        return null;
    }
}