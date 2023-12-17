using System.Net;
using System.Net.Sockets;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

public class SocketListener
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly IServiceProvider _serviceProvider;
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

        // Mapping each operation type to its corresponding handler.
        operationHandlers = new Dictionary<OperationType, Action<NetworkStream, byte[], int>>
        {
            { OperationType.LoginRequest, authService.HandleLoginRequest },
            { OperationType.LogoutRequest, authService.HandleLogoutRequest },
            { OperationType.SignUpRequest, authService.HandleSignUpRequest },
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
            Task.Run(() => HandleNewConnection(client));
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting client connection: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a new client connection.
    /// </summary>
    /// <param name="client">The client that has connected to the server.</param>
    private async Task HandleNewConnection(TcpClient client)
    {
        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
        try
        {
            // Establishes a network stream with the connected client for data communication.
            //using var networkStream = client.GetStream();
            var networkStream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            // Processes the received data.
            HandleReceivedData(networkStream, buffer, bytesRead); // This is now synchronous
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client connection: {ex.Message}");
        }
        finally
        {
            //client.Close();
        }
    }

    /// <summary>
    /// Processes the received data from a connected client.
    /// </summary>
    /// <param name="data">The received data as a byte array.</param>
    /// <param name="bytesRead">The number of bytes actually read from the network stream.</param>
    private void HandleReceivedData(NetworkStream stream, byte[] data, int bytesRead)
    {
        // Check if the received data is less than the size of an integer.
        if (bytesRead < sizeof(int))
        {
            Console.WriteLine("Received data is too short.");
            return;
        }

        try
        {
            // Deserialize the data into a BasePack object to extract the OperationType.
            var basePack = MessagePackSerializer.Deserialize<BasePack>(data);
            OperationType operationType = (OperationType)basePack.OperationTypeId;
            Console.WriteLine($"OperationType: {operationType}");

            // Check if the operation type is defined in the OperationType enum.
            if (!Enum.IsDefined(typeof(OperationType), operationType))
            {
                Console.WriteLine($"Invalid OperationType received: {operationType}");
                return;
            }

            // Call the appropriate handler based on the OperationType.
            InvokeHandlerForOperationType(stream, operationType, data, bytesRead);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing operation type: {ex.Message}");
        }
    }

    /// <summary>
    /// Invokes the handler associated with the specified operation type.
    /// </summary>
    /// <param name="operationType">The operation type to handle.</param>
    /// <param name="data">The received data as a byte array.</param>
    /// <param name="bytesRead">The number of bytes read from the network stream.</param>
    private void InvokeHandlerForOperationType(NetworkStream stream, OperationType operationType, byte[] data, int bytesRead)
    {
        // Attempt to find the handler for the given operation type in the operationHandlers dictionary.
        var handler = operationHandlers?.TryGetValue(operationType, out var tempHandler) == true ? tempHandler : null;

        if (handler != null)
        {
            try
            {
                // Call the handler with the received data and byte count.
                handler(stream, data, bytesRead);
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
}
