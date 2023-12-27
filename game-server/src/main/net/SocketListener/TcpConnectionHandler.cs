using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using MessagePack;

public class TcpConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private Dictionary<OperationType, Action<NetworkStream, byte[], int>>? _operationHandlers;
    private int _connectionId;
    private string _lobbyId;
    private readonly PositionManager _positionManager;
    private readonly ConnectionMasterServer _connectionMasterServer;

    public TcpConnectionHandler(TcpClient client, int connectionId, PositionManager positionManager,
                                    ConnectionMasterServer connectionMasterServer)
    {
        _client = client;
        _stream = client.GetStream();
        _connectionId = connectionId;
        _positionManager = positionManager;
        _connectionMasterServer = connectionMasterServer;
        InitializeTcpOperationHandlers();
    }

    private void InitializeTcpOperationHandlers()
    {
        _operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], int>>
        {
            { OperationType.PlayerLobbyInfo, PlayerLobbyInfo }, // New: PlayerLobbyInfo
            { OperationType.HeartbeatPing, HeartbeatPing }, // New: HeartbeatPing
            { OperationType.GameOverRequest, GameOverRequest }, // New: HeartbeatPing
            { OperationType.LeaveLobbyRequest, HandlePlayerLeavingLobby },
            { OperationType.PlayerJoinedLobbyRequest, HandlePlayerJoinedLobby }

        };
    }

    private void HandlePlayerJoinedLobby(NetworkStream stream, byte[] data, int arg3)
    {
        var playerJoinedLobbyRequest = MessagePackSerializer.Deserialize<PlayerJoinedLobbyRequest>(data);

        _connectionId = playerJoinedLobbyRequest.PlayerID;
        _lobbyId = playerJoinedLobbyRequest.LobbyID;
        Console.WriteLine($"Player {_connectionId} connected.");
    }

    /// <summary>
    /// Handles a heartbeat ping from the server.
    /// </summary>
    public void HeartbeatPing(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var heartbeatMessage = MessagePackSerializer.Deserialize<HeartbeatMessage>(data);
            // Update the connection master server with the stream
            Console.WriteLine($"Heartbeat received from server {heartbeatMessage.ServerID} at {heartbeatMessage.Timestamp}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing heartbeat message: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a game over request from the server. 
    /// This is sent by the server to notify the client that the game has ended.
    /// </summary>
    public async void GameOverRequest(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var gameOverRequest = MessagePackSerializer.Deserialize<GameOverPack>(data);
            var lobbyId = gameOverRequest.LobbyId;
            var winnerPlayerId = gameOverRequest.WinnerPlayerId;
            Console.WriteLine($"GameOverRequest received from server {lobbyId} at {winnerPlayerId}.");
            EndGame(lobbyId ?? string.Empty, winnerPlayerId);
            CleanupLobby(lobbyId ?? string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GameOverRequest: {ex}");
        }
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
    public void EndGame(string lobbyId, int playerId)
    {
        var gameEndData = new GameSavePack
        {
            OperationTypeId = (int)OperationType.GameEndData,
            LobbyID = lobbyId,
            EndTime = DateTime.UtcNow,
            PlayerID = playerId,
        };

        var data = MessagePackSerializer.Serialize(gameEndData);

        SendDataToMasterServer(data);
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

    private void CleanupLobby(string lobbyId)
    {
        if (_positionManager.LobbyExists(lobbyId))
        {
            var playerIds = new List<int>(_positionManager.GetPlayerIdsInLobby(lobbyId));
            foreach (var playerId in playerIds)
            {
                _positionManager.RemovePlayer(playerId);
            }
            _positionManager.RemoveLobby(lobbyId);
        }
    }

    /// <summary>
    /// Handles a player lobby info message from the server.
    /// </summary>
    public async void PlayerLobbyInfo(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var playerLobbyInfo = MessagePackSerializer.Deserialize<PlayerLobbyInfo>(data);
            var response = new PlayerLobbyInfoResponse
            {
                OperationTypeId = (int)OperationType.PlayerLobbyInfoResponse
            };
            //var _endpoint = _client.Client.RemoteEndPoint as IPEndPoint;
            IPEndPoint remoteEndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
            var playerData = new PlayerData
            {
                PlayerId = playerLobbyInfo.PlayerId,
                LobbyId = playerLobbyInfo.LobbyId,
                EndPoint = remoteEndPoint ?? throw new InvalidOperationException("Endpoint is null."),
            };
            _connectionId = playerLobbyInfo.PlayerId;
            Console.WriteLine($"Player {_connectionId} joined lobby {playerLobbyInfo.LobbyId}.");
            // Add the player to the position manager
            _positionManager.AddOrUpdatePlayer(playerLobbyInfo.PlayerId, playerData);
            response.Success = true;
            var responseData = MessagePackSerializer.Serialize(response);
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddOrUpdatePlayer: {ex}");
        }
    }

    /// <summary>
    /// Handles a player leaving lobby message from the server.
    /// This is sent by the server to notify the client that the player has left the lobby.
    /// </summary>
    public void HandlePlayerLeavingLobby(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var playerLeavingLobbyRequest = MessagePackSerializer.Deserialize<PlayerLeavingLobbyRequest>(data);
            var lobbyId = playerLeavingLobbyRequest.LobbyID;
            var playerId = playerLeavingLobbyRequest.PlayerID;
            Console.WriteLine($"Player {playerId} left lobby {lobbyId}.");
            _positionManager.RemovePlayer(playerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HandlePlayerLeavingLobby: {ex}");
        }
    }

    /// <summary>
    /// Handles a new client connection.
    /// </summary>
    /// <param name="client">The client that has connected to the server.</param>
    public void HandleNewConnection()
    {
        byte[] buffer = new byte[1024];
        try
        {
            _stream.BeginRead(buffer, 0, buffer.Length, ReadCallBack, buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting to read from client: {ex.Message}");
            CloseConnection();
        }
    }

    /// <summary>
    /// Callback method for handling data received from a connected client.
    /// </summary>
    /// <param name="ar"></param>
    private void ReadCallBack(IAsyncResult ar)
    {
        if (ar.AsyncState is byte[] buffer)
        {
            try
            {
                int bytesRead = _stream.EndRead(ar);
                if (bytesRead > 0)
                {
                    HandleReceivedData(buffer, bytesRead, _connectionId);
                    _stream.BeginRead(buffer, 0, buffer.Length, ReadCallBack, buffer);
                }
                else
                {
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during read from client: {ex.Message}");
                CloseConnection();
            }
        }
        else
        {
            Console.WriteLine("AsyncState is not a byte array or is null.");
            CloseConnection();
        }
    }

    /// <summary>
    /// Processes the received data from a connected client.
    /// </summary>
    /// <param name="data">The received data as a byte array.</param>
    /// <param name="bytesRead">The number of bytes actually read from the network stream.</param>
    private void HandleReceivedData(byte[] data, int bytesRead, int connectionId)
    {
        // Check if the received data is less than the size of an integer.
        if (bytesRead < sizeof(int))
        {
            Console.WriteLine("Received data is too short.");
            return;
        }
        try
        {
            // Deserialize the data into a BasePack object to extract the OperationType.
            var basePack = MessagePackSerializer.Deserialize<BasePack>(data);
            OperationType operationType = (OperationType)basePack.OperationTypeId;
            Console.WriteLine($"OperationType: {operationType}");

            // Check if the operation type is defined in the OperationType enum.
            if (!Enum.IsDefined(typeof(OperationType), operationType))
            {
                Console.WriteLine($"Invalid OperationType received: {operationType}");
                return;
            }

            // Call the appropriate handler based on the OperationType.
            InvokeHandlerForOperationType(operationType, data, connectionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing operation type: {ex.Message}");
        }
    }

    /// <summary>
    /// Invokes the handler associated with the specified operation type.
    /// </summary>
    /// <param name="operationType">The operation type to handle.</param>
    /// <param name="data">The received data as a byte array.</param>
    /// <param name="bytesRead">The number of bytes read from the network stream.</param>
    private void InvokeHandlerForOperationType(OperationType operationType, byte[] data, int connectionId)
    {
        // Attempt to find the handler for the given operation type in the operationHandlers dictionary.
        var handler = _operationHandlers?.TryGetValue(operationType, out var tempHandler) == true ? tempHandler : null;

        if (handler != null)
        {
            try
            {
                // Call the handler with the received data and byte count.
                handler(_stream, data, connectionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing handler for operation {operationType}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"No handler found for operation type: {operationType}");
        }
    }

    private void CloseConnection()
    {
        _stream.Close();
        _client.Close();
        _positionManager.RemovePlayer(_connectionId);
    }
}