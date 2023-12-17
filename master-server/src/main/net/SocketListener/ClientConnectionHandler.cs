using System.Net.Sockets;
using MessagePack;

public class ClientConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly Dictionary<OperationType, Action<NetworkStream, byte[], int>>? _operationHandlers;

    public ClientConnectionHandler(TcpClient client, Dictionary<OperationType, Action<NetworkStream, byte[], int>>? operationHandlers)    {
        _client = client;
        //using var networkStream = client.GetStream();
        _stream = client.GetStream();
        _operationHandlers = operationHandlers;
    }

    /// <summary>
    /// Handles a new client connection.
    /// </summary>
    /// <param name="client">The client that has connected to the server.</param>
    public async Task HandleNewConnection()
    {
        Console.WriteLine($"Client connected: {_client.Client.RemoteEndPoint}");
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            // Processes the received data.
            HandleReceivedData(buffer, bytesRead); // This is now synchronous
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
    private void HandleReceivedData(byte[] data, int bytesRead)
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
            InvokeHandlerForOperationType(operationType, data, bytesRead);
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
    private void InvokeHandlerForOperationType(OperationType operationType, byte[] data, int bytesRead)
    {
        // Attempt to find the handler for the given operation type in the operationHandlers dictionary.
        var handler = _operationHandlers?.TryGetValue(operationType, out var tempHandler) == true ? tempHandler : null;

        if (handler != null)
        {
            try
            {
                // Call the handler with the received data and byte count.
                handler(_stream, data, bytesRead);
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

    private void CloseConnection()
    {
        _stream.Close();
        _client.Close();
    }
}
