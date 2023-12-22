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

    public void HandlePlayerPositionUpdate(IPEndPoint senderEndPoint, byte[] data)
    {
        try
        {
            var playerPositionUpdate = MessagePackSerializer.Deserialize<PlayerPositionUpdate>(data);
            int playerId = playerPositionUpdate.PlayerId;

            Console.WriteLine($"Received player position update from player {playerId}.");
            Console.WriteLine($"X: {playerPositionUpdate.X}, Y: {playerPositionUpdate.Y}");
            UpdatePlayerPosition(playerId, playerPositionUpdate.X, playerPositionUpdate.Y);
            BroadcastPlayerPositionToLobby(playerId, playerPositionUpdate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling player position update: {ex}");
        }
    }

    private void BroadcastPlayerPositionToLobby(int playerId, PlayerPositionUpdate playerPositionUpdate)
    {
        var data = MessagePackSerializer.Serialize(playerPositionUpdate);

        if (_positionManager.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            // Broadcast the position update to all players in the same lobby
            _positionManager.SendMessageToLobby(playerId, playerData.LobbyId, data);
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
}