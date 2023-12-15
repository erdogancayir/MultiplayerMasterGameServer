
using System;
using System.Net;
using System.Net.Sockets;

public class SocketListener
{
    private readonly int _port;

    public SocketListener(int port)
    {
        _port = port;
    }

    public void Start()
    {
        // Start listening for incoming connections on the specified port
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"Listening for connections on port {_port}...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            // Handle the new connection (e.g., authenticate, establish session)
            HandleNewConnection(client);
        }
    }

    private void HandleNewConnection(TcpClient client)
    {
        // Logic to handle new client connection
        // This might involve authentication and session initiation
    }
}
