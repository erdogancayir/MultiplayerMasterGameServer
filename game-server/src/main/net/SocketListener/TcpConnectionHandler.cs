using System.Net;
using System.Net.Sockets;
using MessagePack;

public class TcpConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private Dictionary<OperationType, Action<NetworkStream, byte[], int>>? _operationHandlers;
    private TcpConnectionManager _tcpConnectionManager;
    private int _connectionId;
    private readonly PositionManager _positionManager;


    public TcpConnectionHandler(TcpClient client, TcpConnectionManager connectionManager, int connectionId, PositionManager positionManager) {
        _client = client;
        //using var networkStream = client.GetStream();
        _stream = client.GetStream();
        _tcpConnectionManager = connectionManager;
        _connectionId = connectionId;
        _positionManager = positionManager;
        InitializeTcpOperationHandlers();
    }

    private void InitializeTcpOperationHandlers()
    {
        _operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], int>>
        {
            { OperationType.PlayerLobbyInfo, PlayerLobbyInfo },
            { OperationType.HeartbeatPing, HeartbeatPing }
        };
    }

    /// <summary>
    /// Handles a heartbeat ping from the server.
    /// </summary>
    public void HeartbeatPing(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var heartbeatMessage = MessagePackSerializer.Deserialize<HeartbeatMessage>(data);
            Console.WriteLine($"Heartbeat received from server {heartbeatMessage.ServerID} at {heartbeatMessage.Timestamp}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing heartbeat message: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a player lobby info message from the server.
    /// </summary>
    public void PlayerLobbyInfo(NetworkStream stream, byte[] data, int connectionId)
    {
        try
        {
            var playerLobbyInfo = MessagePackSerializer.Deserialize<PlayerLobbyInfo>(data);
            Console.WriteLine($"PlayerLobbyInfo: {playerLobbyInfo.PlayerId} {playerLobbyInfo.LobbyId}");

            var _endpoint = _client.Client.RemoteEndPoint as IPEndPoint;

            var playerData = new PlayerData
            {
                PlayerId = playerLobbyInfo.PlayerId,
                LobbyId = playerLobbyInfo.LobbyId,
                EndPoint = _endpoint ?? throw new InvalidOperationException("Endpoint is null."),
            };
            // Add the player to the position manager
            _positionManager.AddOrUpdatePlayer(playerLobbyInfo.PlayerId, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddOrUpdatePlayer: {ex}");
        }
    }

    /// <summary>
    /// Handles a new client connection.
    /// </summary>
    /// <param name="client">The client that has connected to the server.</param>
    public void HandleNewConnection()
    {
        Console.WriteLine($"Client connected: {_client.Client.RemoteEndPoint}");
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
        _tcpConnectionManager.RemoveConnection(_connectionId);
    }
}