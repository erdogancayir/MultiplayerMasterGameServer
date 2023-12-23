using System.Net;
using System.Net.Sockets;
using MessagePack;

public class UdpConnectionHandler
{
    private UdpClient _udpClient; // UDP client for receiving data
    private readonly int _udpPort;
    private Dictionary<OperationType, Action<IPEndPoint, byte[]>>? _udpOperationHandlers;
    private readonly PositionManager _positionManager;
    private readonly ConnectionMasterServer _connectionMasterServer;

    public UdpConnectionHandler(int udpPort, 
                                PositionManager positionManager,
                                ConnectionMasterServer connectionMasterServer)
    {
        _udpPort = udpPort;
        _udpClient = new UdpClient(_udpPort);
        _positionManager = positionManager;
        _connectionMasterServer = connectionMasterServer;
        InitializeUdpOperationHandlers();
    }

    private void InitializeUdpOperationHandlers()
    {
        _udpOperationHandlers = new Dictionary<OperationType, Action<IPEndPoint, byte[]>>
        {
            { OperationType.PlayerPositionUpdate, HandlePlayerPositionUpdate }
        };
    }

    public void StartListening()
    {
        try
        {
            BeginReceiveUdp();
            Console.WriteLine($"UDP Listener started on port {_udpPort}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting UDP listener: {ex}");
        }
    }

    private void BeginReceiveUdp()
    {
        _udpClient.BeginReceive(UdpReceiveCallback, null);
    }

    private void UdpReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = _udpClient.EndReceive(ar, ref remoteEndPoint);

            if (remoteEndPoint != null)
            {
                ParseAndHandleUdpData(receivedBytes, remoteEndPoint);
                BeginReceiveUdp();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving UDP packet: {ex}");
            BeginReceiveUdp();
        }
    }

    /// <summary>
    /// Parses the received bytes and calls the corresponding handler.
    /// </summary>
    /// <param name="receivedBytes"></param>
    /// <param name="senderEndPoint"></param>
    private void ParseAndHandleUdpData(byte[] receivedBytes, IPEndPoint senderEndPoint)
    {
        try 
        {
            var basePack = MessagePackSerializer.Deserialize<BasePack>(receivedBytes);
            if (!Enum.IsDefined(typeof(OperationType), basePack.OperationTypeId))
            {
                Console.WriteLine($"Invalid OperationType received: {basePack.OperationTypeId}");
                return;
            }

            if (_udpOperationHandlers != null && _udpOperationHandlers.TryGetValue((OperationType)basePack.OperationTypeId, out var handler))
            {
                handler(senderEndPoint, receivedBytes);
            }
            else
            {
                Console.WriteLine($"No handler found for operation type: {basePack.OperationTypeId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing UDP data: {ex}");
        }
    }

    /// <summary>
    /// Handles a player position update.
    /// </summary>
    /// <param name="senderEndPoint"></param>
    /// <param name="data"></param>
    public void HandlePlayerPositionUpdate(IPEndPoint senderEndPoint, byte[] data)
    {
        try
        {
            var playerPositionUpdate = MessagePackSerializer.Deserialize<PlayerPositionUpdate>(data);
            int playerId = playerPositionUpdate.PlayerId;
            string lobbyId = _positionManager.GetPlayerLobbyId(playerId) ?? "";

            Console.WriteLine($"Received player position update from player {playerId}.");
            Console.WriteLine($"X: {playerPositionUpdate.X}, Y: {playerPositionUpdate.Y}");
            UpdatePlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPositionToLobby(playerId, playerPositionUpdate);
            if (CheckGameEndCondition(playerPositionUpdate.X, playerPositionUpdate.Y))
            {
                EndGame(lobbyId, playerId);
                //BroadcastGameEnd();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling player position update: {ex}");
        }
    }

    /// <summary>
    /// Broadcasts the player position update to all players in the same lobby.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="playerPositionUpdate"></param>
    private async void BroadcastPlayerPositionToLobby(int playerId, PlayerPositionUpdate playerPositionUpdate)
    {
        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            // Broadcast the position update to all players in the same lobby
            if (playerData.LobbyId != null)
                await _positionManager.SendMessageToLobby(playerId, playerData.LobbyId, data);
        }
    }

    /// <summary>
    /// Updates the player position in the position manager.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void UpdatePlayerPosition(int playerId, float x, float y)
    {
        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            playerData.X = x;
            playerData.Y = y;
            _positionManager.AddOrUpdatePlayer(playerId, playerData);
        }
    }

    /// <summary>
    /// Checks if the game end condition is met.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private bool CheckGameEndCondition(float x, float y)
    {
        float   TargetX = 100;
        float   TargetY = 100;

        return (x == TargetX && y == TargetY);
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <param name="playerId"></param>
    public void EndGame(string lobbyId, int playerId)
    {
        Game gameData = CreateGameData(lobbyId);

        var gameEndData = new GameSavePack
        {
            OperationTypeId = (int)OperationType.GameEndData,
            GameData = gameData,
            PlayerID = playerId
        };

        var data = MessagePackSerializer.Serialize(gameEndData);

        SendDataToMasterServer(data);
    }

    private Game CreateGameData(string lobbyId)
    {
        return new Game
        {
            LobbyID = lobbyId,
            EndTime = DateTime.UtcNow,
            Status = Game.GameStatus.Completed
        };
    }

    public void SendDataToMasterServer(byte[] data)
    {
        try
        {
            _connectionMasterServer.SendData(data);
            var response = _connectionMasterServer.ReceiveData();
            var createGameResponse = MessagePackSerializer.Deserialize<GameSaveResponsePack>(response);
            if (createGameResponse.Success)
                Console.WriteLine($"Game saved.");
            else
                Console.WriteLine($"Failed to save game.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending data to Master Server: {ex}");
        }
    }
}