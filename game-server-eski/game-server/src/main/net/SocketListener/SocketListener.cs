using System.Net;
using System.Net.Sockets;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

public class SocketListener
{
    private readonly int _tcpPort;
    private readonly int _udpPort;
    private TcpListener? _tcpListener;
    private UdpClient? _udpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly TcpConnectionManager _connectionManager;
    private Dictionary<OperationType, Action<NetworkStream ,byte[], string>>? _operationHandlers;

    public SocketListener(int tcpPort, int udpPort, IServiceProvider serviceProvider)
    {
        _tcpPort = tcpPort;
        _udpPort = udpPort;
        _serviceProvider = serviceProvider;
        _connectionManager = _serviceProvider.GetRequiredService<TcpConnectionManager>();
        InitializeOperationHandlers();
    }


    /// <summary>
    /// Initializes the operation handlers for different types of operations.
    /// </summary>
    private void InitializeOperationHandlers()
    {
        _operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], string>>
        {
            {OperationType.PlayerPositionUpdate, HandlePlayerPositionUpdate},
        };
    }

    public void Start()
    {
        StartTcpListener();
        StartUdpListener();
    }

    private void StartTcpListener()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, _tcpPort);
            _tcpListener.Start();
            Console.WriteLine($"TCP Listener started on port {_tcpPort}.");

            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpClientAccepted), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting the server: {ex.Message}");
        }
    }

    private void TcpClientAccepted(IAsyncResult ar)
    {
        try
        {
            if (_tcpListener == null)
            {
                throw new InvalidOperationException("TCP Listener is not initialized.");
            }
            // End accepting the TCP client
            TcpClient tcpClient = _tcpListener.EndAcceptTcpClient(ar);

            // Get the connection temp ID for the client
            string connectionId = Guid.NewGuid().ToString();

            // Create a new TCP connection handler for the client
            var clientConnection = new TcpConnectionHandler(tcpClient, _operationHandlers, _connectionManager, connectionId);

            // Handle the TCP client in a new task or thread
            Task.Run(() => clientConnection.HandleNewConnection());

            // Continue accepting other clients
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpClientAccepted), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting client connection: {ex.Message}");
        }
    }

    private void StartUdpListener()
    {
        try
        {
            _udpClient = new UdpClient(_udpPort);
            Console.WriteLine($"UDP Listener started on port {_udpPort}.");

            BeginReceiveUdp();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting the UDP server: {ex.Message}");
        }
    }

    private void BeginReceiveUdp()
    {
        if (_udpClient == null)
        {
            throw new InvalidOperationException("UDP Listener is not initialized.");
        }
        _udpClient.BeginReceive(UdpReceiveCallback, null);
    }

    private void UdpReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
            if (_udpClient == null)
            {
                throw new InvalidOperationException("UDP Listener is not initialized.");
            }
            byte[] receivedBytes = _udpClient.EndReceive(ar, ref remoteEndPoint);

            ParseAndHandleUdpData(receivedBytes);
            // Continue receiving
            BeginReceiveUdp();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving UDP packet: {ex.Message}");
        }
    }

    private void ParseAndHandleUdpData(byte[] receivedBytes)
    {
        try 
        {
            var basePack = MessagePackSerializer.Deserialize<BasePack>(receivedBytes);
            OperationType operationType = (OperationType)basePack.OperationTypeId;
            Console.WriteLine($"Udp OperationType: {operationType}");

            // Check if the operation type is defined in the OperationType enum.
            if (!Enum.IsDefined(typeof(OperationType), operationType))
            {
                Console.WriteLine($"Invalid OperationType received: {operationType}");
                return;
            }

            var handler = _operationHandlers?.TryGetValue(operationType, out var tempHandler) == true ? tempHandler : null;
            if (handler != null)
            {
                try
                {
                    // Call the handler with the received data and byte count.
                    handler(null, receivedBytes, "");
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing UDP data: {ex.Message}");
            return;
        }
    }

    private void HandlePlayerPositionUpdate(NetworkStream clientStream, byte[] data, string connectionId)
    {
        // Parse the data to get player ID and new position
        var signUpRequest = MessagePackSerializer.Deserialize<PlayerPosition>(data);

        // Update the player's position in the game world
        // Assuming a method UpdatePlayerPosition exists
        //_gameSessionManager.UpdatePlayerPosition(playerPosition.PlayerId, playerPosition.NewPosition);

        // Optionally, broadcast the new position to other players
    }

}
