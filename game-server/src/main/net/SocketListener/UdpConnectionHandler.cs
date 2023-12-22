using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MessagePack;
using Microsoft.Extensions.Logging;
using Amazon.Runtime.Internal;
using System.ComponentModel; // Varsayılan olarak bu kütüphaneyi ekleyin

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
            int playerId = playerPositionUpdate.PlayerId;

            UpdatePlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPositionToLobby(playerId, playerPositionUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling player position update: {ex}");
        }
    }

    private void BroadcastPlayerPositionToLobby(int playerId, PlayerPositionUpdate playerPositionUpdate)
    {
        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            // Broadcast the position update to all players in the same lobby
            _positionManager.SendMessageToLobby(playerData.LobbyId, data);
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
            var playerLobbyInfo = MessagePackSerializer.Deserialize<PlayerLobbyInfo>(receivedBytes);

            var playerData = new PlayerData
            {
                PlayerId = playerLobbyInfo.PlayerId,
                LobbyId = playerLobbyInfo.LobbyId,
                EndPoint = endPoint,
                X = playerLobbyInfo.X,
                Y = playerLobbyInfo.Y
            };

            _positionManager.AddOrUpdatePlayer(playerLobbyInfo.PlayerId, playerData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AddOrUpdatePlayer: {ex}");
        }
    }
}