using System.Net;
using System.Net.Sockets;
using MessagePack;

public class UdpConnectionHandler
{
    private UdpClient _udpClient;
    private readonly int _udpPort;
    private Dictionary<OperationType, Action<IPEndPoint, byte[]>>? _udpOperationHandlers;
    private readonly PositionManager _positionManager;

    public UdpConnectionHandler(int udpPort, 
                                PositionManager positionManager)
    {
        _udpPort = udpPort;
        _udpClient = new UdpClient(_udpPort);
        _positionManager = positionManager;
        InitializeUdpOperationHandlers();
    }

    private void InitializeUdpOperationHandlers()
    {
        _udpOperationHandlers = new Dictionary<OperationType, Action<IPEndPoint, byte[]>>
        {
            { OperationType.PlayerPositionUpdate, HandlePlayerPositionUpdate },
            { OperationType.EndPoint, HandlePlayerEndPointUpdate }
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
            _positionManager.SetUdpClient(_udpClient);
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

            UpdatePlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPositionToLobby(playerId, playerPositionUpdate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling player position update: {ex}");
        }
    }

    public void HandlePlayerEndPointUpdate(IPEndPoint senderEndPoint, byte[] data)
    {
        try
        {
            var endPoint = MessagePackSerializer.Deserialize<EndPointPack>(data);
            int playerId = endPoint.PlayerId;

            _positionManager.updatePlayerEndPoint(senderEndPoint, playerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling player EndPoint update: {ex}");
        }
    }

    /// <summary>
    /// Broadcasts the player position update to all players in the same lobby.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="playerPositionUpdate"></param>
    private void BroadcastPlayerPositionToLobby(int playerId, PlayerPositionUpdate playerPositionUpdate)
    {
        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            // Broadcast the position update to all players in the same lobby
            if (playerData.LobbyId != null)
                _positionManager.SendMessageToLobby(playerId, playerData.LobbyId, data);
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
}