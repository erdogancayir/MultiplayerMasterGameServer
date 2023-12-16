using System.Net;
using System.Net.Sockets;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

public class SocketListener
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<OperationType, Action<byte[], int>>? operationHandlers;

    public SocketListener(int port, IServiceProvider serviceProvider)
    {
        _port = port;
        _serviceProvider = serviceProvider;
        _listener = new TcpListener(IPAddress.Any, _port);
        InitializeOperationHandlers();
    }

    private void InitializeOperationHandlers()
    {
        var authService = _serviceProvider.GetService<IAuthService>()
                     ?? throw new InvalidOperationException("AuthService not available.");

        operationHandlers = new Dictionary<OperationType, Action<byte[], int>>
        {
            { OperationType.LoginRequest, authService.HandleLoginRequest },
            { OperationType.LogoutRequest, authService.HandleLogoutRequest },
            // ...diğer eşleştirmeler...
        };
    }
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

    private void BeginAcceptClient()
    {
        _listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
    }

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
    private async Task HandleNewConnection(TcpClient client)
    {
        try
        {
            using var networkStream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            HandleReceivedData(buffer, bytesRead); // This is now synchronous
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client connection: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private void HandleReceivedData(byte[] data, int bytesRead)
    {
        try
        {
            // Deserialize to a generic packet structure (assuming it contains an operation type)
            var packet = MessagePackSerializer.Deserialize<GenericPacket>(new Memory<byte>(data, 0, bytesRead));
            if (operationHandlers?.TryGetValue(packet.OperationType, out var handler) == true)
            {
                // Invoke the handler
                handler(data, bytesRead);
            }
            else
            {
                Console.WriteLine($"No handler found for operation type: {packet.OperationType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing received data: {ex.Message}");
        }
    }
}
