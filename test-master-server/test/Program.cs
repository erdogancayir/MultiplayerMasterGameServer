﻿using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

public class TcpClientExample
{
    public static async Task Main(string[] args)
    {
        string server = "127.0.0.1"; // Master Server IP
        int port = 8080; // Master Server Port

        var client = new TcpClient();

        try
        {
            // Connect to the server
            await client.ConnectAsync(server, port);

            // Create a request
            var request = new PlayerRequest
            {
                Username = "Player1",
                Action = "JoinGame"
                // Set other properties as needed
            };

            // Serialize the request
            byte[] dataToSend = MessagePackSerializer.Serialize(request);

            // Get the network stream
            NetworkStream stream = client.GetStream();

            // Send the serialized data
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

            Console.WriteLine("Data sent to the server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}