
using System;
using System.Net;
using System.Net.Sockets;
using MessagePack;

public class SocketListener
{
    private readonly int _port;
    private readonly PlayerManager _playerManager;
    private readonly TcpListener _listener;

    public SocketListener(int port, PlayerManager playerManager)
    {
        _port = port;
        _playerManager = playerManager;
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    public void Start()
    {
        try
        {
            _listener.Start();
            Console.WriteLine($"Listening for connections on port {_port}...");

            // Start an asynchronous operation to accept client connections.
            _listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error starting the server: {ex.Message}");
        }
    }

    private void TcpConnectCallback(IAsyncResult ar)
    {
        try
        {
            TcpClient client = _listener.EndAcceptTcpClient(ar);

            // Handle the new connection in a separate task
            Task.Run(() => HandleNewConnection(client));

            // Continue listening for other client connections
            _listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error accepting client connection: {ex.Message}");
        }
    }
    private void HandleNewConnection(TcpClient client)
    {
        try
        {
            using var networkStream = client.GetStream();
            byte[] buffer = new byte[1024]; // Adjust size as needed
            int bytesRead;

            // Read data from the stream
            bytesRead = networkStream.Read(buffer, 0, buffer.Length);

            HandleReceivedData(buffer, bytesRead);
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
            // Convert the byte array to a Memory<byte> for deserialization
            var memoryData = new Memory<byte>(data, 0, bytesRead);

             // Deserialize the data using MessagePack
            var playerRequest = MessagePackSerializer.Deserialize<PlayerRequest>(memoryData);

            Console.WriteLine($"Received data: {playerRequest}");
            // Act based on the deserialized data
            // Example: if (playerRequest.Action == "JoinGame") { ... }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing received data: {ex.Message}");
        }
    }

}
