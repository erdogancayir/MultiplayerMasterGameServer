using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MessagePack;
using Microsoft.Extensions.Logging; // Varsayılan olarak bu kütüphaneyi ekleyin

public class UdpConnectionHandler
{
    private UdpClient _udpClient;
    private readonly int _udpPort;
    private ConcurrentDictionary<string, UdpConnectionManager> _connectedPlayers;
    private Dictionary<OperationType, Action<IPEndPoint, byte[]>>? _udpOperationHandlers;
    private readonly UdpConnectionManager _udpConnectionManager;
    private readonly ILogger<UdpConnectionHandler> _logger;

    public UdpConnectionHandler(int udpPort, UdpConnectionManager udpConnectionManager, 
                                Dictionary<OperationType, Action<IPEndPoint, byte[]>>? udpOperationHandlers, 
                                ILogger<UdpConnectionHandler> logger)
    {
        _udpPort = udpPort;
        _udpClient = new UdpClient(_udpPort);
        _udpOperationHandlers = udpOperationHandlers;
        _udpConnectionManager = udpConnectionManager;
        _connectedPlayers = new ConcurrentDictionary<string, UdpConnectionManager>();
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

            UpdatePlayerPosition(playerPositionUpdate.PlayerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPosition(playerPositionUpdate.PlayerId, playerPositionUpdate.X, playerPositionUpdate.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling player position update: {ex}");
        }
    }


     // Placeholder for broadcasting player position to other players
    private void BroadcastPlayerPosition(string playerId, float x, float y)
    {
        var playerPositionUpdate = new PlayerPositionUpdate
        {
            PlayerId = playerId,
            X = x,
            Y = y
        };

        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        foreach (var player in _connectedPlayers.Values)
        {
            if (player.PlayerId != playerId)
            {
                // SendMessage metodu, UDP üzerinden veri gönderme işlemini gerçekleştirmeli
                player.SendMessage(data);
            }
        }
    }


    // Oyuncu pozisyonunu güncellemek için
    public void UpdatePlayerPosition(string playerId, float x, float y)
    {
        if (_connectedPlayers.TryGetValue(playerId, out var udpConnection))
        {
            udpConnection.X = x;
            udpConnection.Y = y;
        }
    }
    private void AddOrUpdatePlayer(IPEndPoint endPoint, byte[] receivedBytes)
    {
        try
        {
            var playerPositionUpdate = MessagePackSerializer.Deserialize<PlayerPositionUpdate>(receivedBytes);
            string playerId = playerPositionUpdate.PlayerId;

            if (!_connectedPlayers.TryGetValue(playerId, out var udpConnection))
            {
                // add the new player to the connected players dictionary
                udpConnection = new UdpConnectionManager
                {
                    PlayerId = playerId,
                    EndPoint = endPoint,
                    X = playerPositionUpdate.X,
                    Y = playerPositionUpdate.Y
                };
                _connectedPlayers[playerId] = udpConnection;
            }
            else
            {
                // update the existing player's position
                udpConnection.X = playerPositionUpdate.X;
                udpConnection.Y = playerPositionUpdate.Y;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AddOrUpdatePlayer: {ex}");
        }
    }
}