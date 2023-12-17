using System.Net;
using System.Net.Sockets;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

public class SocketListener
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConnectionManager _connectionManager;
    private Dictionary<OperationType, Action<NetworkStream ,byte[], int>>? operationHandlers;
    
    /// <summary>
    /// Initializes a new instance of the SocketListener class.
    /// </summary>
    /// <param name="port">The port number on which the server will listen for incoming connections.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public SocketListener(int port, IServiceProvider serviceProvider)
    {
        _port = port;
        _serviceProvider = serviceProvider;
        _listener = new TcpListener(IPAddress.Any, _port);
        _connectionManager = _serviceProvider.GetService<ConnectionManager>()
                     ?? throw new InvalidOperationException("ConnectionManager not available.");
        InitializeOperationHandlers();
    }

    /// <summary>
    /// Initializes the operation handlers for different types of operations.
    /// </summary>
    private void InitializeOperationHandlers()
    {
        // Retrieves the authentication service from the service provider or throws an exception if not available.
        var authService = _serviceProvider.GetService<IAuthService>()
                     ?? throw new InvalidOperationException("AuthService not available.");
        var lobbyManager = _serviceProvider.GetService<LobbyManager>()
                     ?? throw new InvalidOperationException("LobbyManager not available.");
        var matchmaker = _serviceProvider.GetService<Matchmaker>()
                     ?? throw new InvalidOperationException("Matchmaker not available.");

        // Mapping each operation type to its corresponding handler.
        operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], int>>
        {
            { OperationType.LoginRequest, authService.HandleLoginRequest },
            { OperationType.LogoutRequest, authService.HandleLogoutRequest },
            { OperationType.SignUpRequest, authService.HandleSignUpRequest },
            { OperationType.JoinLobbyRequest, matchmaker.HandleJoinLobbyRequest },
            // ... other mappings ...
        };
    }

    /// <summary>
    /// Starts the TCP listener to accept incoming client connections.
    /// </summary>
    public void Start()
    {
        try
        {
            _listener.Start();
            Console.WriteLine($"Listening for connections on port {_port}...");
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting the server: {ex.Message}");
        }
    }

    /// <summary>
    /// Begins an asynchronous operation to accept an incoming client connection attempt.
    /// </summary>
    private void BeginAcceptClient()
    {
        _listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
    }

    /// <summary>
    /// Callback method for handling client connection attempts.
    /// </summary>
    /// <param name="ar">The status of the asynchronous operation.</param>
    private void TcpConnectCallback(IAsyncResult ar)
    {
        try
        {
            TcpClient client = _listener.EndAcceptTcpClient(ar);
            string connectionId = Guid.NewGuid().ToString();

            //son iki parametre eklendi
            var clientConnection = new ClientConnectionHandler(client, operationHandlers, _connectionManager, connectionId);
            
            _connectionManager.AddConnection(connectionId, client);

            Task.Run(() => clientConnection.HandleNewConnection());
            //Task.Run(() => HandleNewConnection(client));
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting client connection: {ex.Message}");
        }
    }

}
