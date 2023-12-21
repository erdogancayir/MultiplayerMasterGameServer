using System.Net;
using System.Net.Sockets;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class SocketListener
{
    private readonly int _tcpPort;
    private readonly int _udpPort;
    private TcpListener? _tcpListener;
    private readonly IServiceProvider _serviceProvider;
    private readonly TcpConnectionManager _tcpConnectionManager;
    private Dictionary<OperationType, Action<NetworkStream ,byte[], string>>? _tcpOperationHandlers;
    private Dictionary<OperationType, Action<IPEndPoint, byte[]>>? _udpOperationHandlers;
    private UdpConnectionHandler _udpConnectionHandler;

    public SocketListener(int tcpPort, int udpPort, IServiceProvider serviceProvider)
    {
        _tcpPort = tcpPort;
        _udpPort = udpPort;
        _serviceProvider = serviceProvider;
        _tcpConnectionManager = _serviceProvider.GetRequiredService<TcpConnectionManager>();
        InitializeUdpOperationHandlers();
        InitializeTcpOperationHandlers();
    }

    private void InitializeTcpOperationHandlers()
    {
        // TCP operation handlers initialization
    }

    private void InitializeUdpOperationHandlers()
    {
        _udpOperationHandlers = new Dictionary<OperationType, Action<IPEndPoint, byte[]>>
        {
            { OperationType.PlayerPositionUpdate, _udpConnectionHandler.HandlePlayerPositionUpdate }
        };
        var udpConnectionHandlerLogger = _serviceProvider.GetRequiredService<ILogger<UdpConnectionHandler>>();
        var positionManager = _serviceProvider.GetRequiredService<PositionManager>();
        _udpConnectionHandler = new UdpConnectionHandler(_udpPort, _udpOperationHandlers, udpConnectionHandlerLogger, positionManager);
    }

    public void Start()
    {
        StartTcpListener();
        _udpConnectionHandler.StartListening();
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
            Console.WriteLine($"Error starting the TCP server: {ex}");
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
            var clientConnection = new TcpConnectionHandler(tcpClient, _tcpOperationHandlers, _tcpConnectionManager, connectionId);

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
}
