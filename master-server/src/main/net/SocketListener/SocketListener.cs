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
    private readonly HeartbeatManager _heartbeatManager;
    private Dictionary<OperationType, Action<NetworkStream ,byte[], int>>? operationHandlers;
    private Random _random = new Random();
    
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
        _heartbeatManager = _serviceProvider.GetService<HeartbeatManager>() ?? throw new InvalidOperationException("HeartbeatManager not available.");
        InitializeOperationHandlers();
    }

    /// <summary>
    /// Initializes the operation handlers for different types of operations.
    /// </summary>
    private void InitializeOperationHandlers()
    {
        // Retrieve services
        var authService = GetService<AuthService>("AuthService");
        var lobbyManager = GetService<LobbyManager>("LobbyManager");
        var matchmaker = GetService<Matchmaker>("Matchmaker");
        var gameManager = GetService<GameManager>("GameManager");
        var gameStatisticsManager = GetService<GameStatisticsManager>("GameStatisticsManager");
        var leaderboardManager = GetService<LeaderboardManager>("LeaderboardManager");

        // Mapping each operation type to its corresponding handler.
        operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], int>>
        {
            { OperationType.LoginRequest, authService.HandleLoginRequest },
            { OperationType.LogoutRequest, authService.HandleLogoutRequest },
            { OperationType.SignUpRequest, authService.HandleSignUpRequest },
            { OperationType.JoinLobbyRequest, matchmaker.HandleJoinLobbyRequest },
            { OperationType.CreateLobbyRequest, matchmaker.CreateLobby },
            { OperationType.GameEndData, gameManager.HandleCreateGameRequest},

            { OperationType.GetGame, gameManager.HandleGetGameRequest},
            { OperationType.GetGameStatistics, gameStatisticsManager.HandleGetGameStatisticsRequest},
            { OperationType.GetTopLeaderboardEntries, leaderboardManager.HandleGetTopLeaderboardEntriesRequest},
        };
    }

    /// <summary>
    /// Retrieves a service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    private T GetService<T>(string serviceName)
    {
        return _serviceProvider.GetService<T>()
               ?? throw new InvalidOperationException($"{serviceName} not available.");
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
            _heartbeatManager.StartSendingHeartbeats();
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

            int connectionId = _random.Next();
            //string connectionId = Guid.NewGuid().ToString();

            // Create a new client connection handler for the client
            var clientConnection = new ClientConnectionHandler(client, operationHandlers, _connectionManager, connectionId);

            _connectionManager.AddConnection(connectionId, client);

            Task.Run(() => clientConnection.HandleNewConnection());
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting client connection: {ex.Message}");
        }
    }

}
