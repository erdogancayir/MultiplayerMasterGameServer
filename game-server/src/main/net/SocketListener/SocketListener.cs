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
    private UdpConnectionHandler _udpConnectionHandler;
    private Random _random = new Random();

    public SocketListener(int tcpPort, int udpPort, IServiceProvider serviceProvider)
    {
        _tcpPort = tcpPort;
        _udpPort = udpPort;
        _serviceProvider = serviceProvider;
        _udpConnectionHandler = new UdpConnectionHandler(_udpPort,
                                                        _serviceProvider.GetRequiredService<PositionManager>(),
                                                        _serviceProvider.GetRequiredService<ConnectionMasterServer>());
        _tcpConnectionManager = _serviceProvider.GetRequiredService<TcpConnectionManager>();
    }

    /// <summary>
    /// Starts the TCP and UDP listeners.
    /// </summary>
    public void Start()
    {
        StartTcpListener();
        _udpConnectionHandler.StartListening();
    }

    /// <summary>
    /// Starts the TCP listener.
    /// </summary>
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

    /// <summary>
    /// Handles a new TCP client connection.
    /// </summary>
    /// <param name="ar"></param>
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
            int connectionId = _random.Next();

            // Create a new TCP connection handler for the client
            var clientConnection = new TcpConnectionHandler(tcpClient, _tcpConnectionManager, connectionId, _serviceProvider.GetRequiredService<PositionManager>());

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
