using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MessagePack;
using Microsoft.Extensions.Logging; // Varsayılan olarak bu kütüphaneyi ekleyin

public class UdpConnectionHandler
{
    private UdpClient _udpClient;
    private readonly int _udpPort;
    private Dictionary<OperationType, Action<IPEndPoint, byte[]>>? _udpOperationHandlers;
    private readonly ILogger<UdpConnectionHandler> _logger;
    private readonly PositionManager _positionManager;

    public UdpConnectionHandler(int udpPort, 
                                Dictionary<OperationType, Action<IPEndPoint, byte[]>>? udpOperationHandlers, 
                                ILogger<UdpConnectionHandler> logger,
                                PositionManager positionManager)
    {
        _udpPort = udpPort;
        _udpClient = new UdpClient(_udpPort);
        _udpOperationHandlers = udpOperationHandlers;
        _positionManager = positionManager;
        _logger = logger;
    }

    public void StartListening()
    {
        try
        {
            BeginReceiveUdp();
            _logger.LogInformation($"UDP Listener started on port {_udpPort}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting UDP listener: {ex}");
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
                AddOrUpdatePlayer(remoteEndPoint, receivedBytes);
                ParseAndHandleUdpData(receivedBytes, remoteEndPoint);
                BeginReceiveUdp();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error receiving UDP packet: {ex}");
            BeginReceiveUdp(); // Yeniden dinlemeye başla
        }
    }

    private void ParseAndHandleUdpData(byte[] receivedBytes, IPEndPoint senderEndPoint)
    {
        try 
        {
            var basePack = MessagePackSerializer.Deserialize<BasePack>(receivedBytes);
            if (!Enum.IsDefined(typeof(OperationType), basePack.OperationTypeId))
            {
                _logger.LogWarning($"Invalid OperationType received: {basePack.OperationTypeId}");
                return;
            }

            if (_udpOperationHandlers != null && _udpOperationHandlers.TryGetValue((OperationType)basePack.OperationTypeId, out var handler))
            {
                handler(senderEndPoint, receivedBytes);
            }
            else
            {
                _logger.LogWarning($"No handler found for operation type: {basePack.OperationTypeId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing UDP data: {ex}");
        }
    }

    public void HandlePlayerPositionUpdate(IPEndPoint senderEndPoint, byte[] data)
    {
        try
        {
            var playerPositionUpdate = MessagePackSerializer.Deserialize<PlayerPositionUpdate>(data);
            int playerId = int.Parse(playerPositionUpdate.PlayerId); // Ensure playerId is an int

            UpdatePlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling player position update: {ex}");
        }
    }

    private void BroadcastPlayerPosition(int playerId, float x, float y)
    {
        var playerPositionUpdate = new PlayerPositionUpdate
        {
            PlayerId = playerId.ToString(),
            X = x,
            Y = y
        };

        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        foreach (var kvp in _positionManager.GetAllPlayers())
        {
            if (kvp.Key != playerId)
            {
                _positionManager.SendMessageToPlayer(kvp.Key, data); // Adjusted to use PositionManager
            }
        }
    }

    public void UpdatePlayerPosition(int playerId, float x, float y)
    {
        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            playerData.X = x;
            playerData.Y = y;
            _positionManager.AddOrUpdatePlayer(playerId, playerData);
        }
    }
    private void AddOrUpdatePlayer(IPEndPoint endPoint, byte[] receivedBytes)
    {
        try
        {
            var playerPositionUpdate = MessagePackSerializer.Deserialize<PlayerPositionUpdate>(receivedBytes);
            int playerId = int.Parse(playerPositionUpdate.PlayerId); // Convert playerId to int

            var playerData = new PlayerData
            {
                PlayerId = playerId,
                EndPoint = endPoint,
                X = playerPositionUpdate.X,
                Y = playerPositionUpdate.Y
                // Add other necessary fields like LobbyId if applicable
            };

            _positionManager.AddOrUpdatePlayer(playerId, playerData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AddOrUpdatePlayer: {ex}");
        }
    }
}