using System.Net.Sockets;
using MessagePack;

public class ClientConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly Dictionary<OperationType, Action<NetworkStream, byte[], string>>? _operationHandlers;
    private ConnectionManager _connectionManager;
    private string _connectionId;

    public ClientConnectionHandler(TcpClient client, Dictionary<OperationType, Action<NetworkStream, byte[], string>>? operationHandlers, ConnectionManager connectionManager, string connectionId)    {
        _client = client;
        //using var networkStream = client.GetStream();
        _stream = client.GetStream();
        _operationHandlers = operationHandlers;
        _connectionManager = connectionManager;
        _connectionId = connectionId;
    }

    /// <summary>
    /// Updates the connection id of the client.
    /// </summary>
    /// <param name="newPlayerId"></param>
    public void UpdateConnectionId(string newPlayerId)
    {
        _connectionManager.UpdateConnectionId(_connectionId, newPlayerId);
        _connectionId = newPlayerId;
    }

    /// <summary>
    /// Handles a new client connection.
    /// </summary>
    /// <param name="client">The client that has connected to the server.</param>
    public void HandleNewConnection()
    {
        Console.WriteLine($"Client connected: {_client.Client.RemoteEndPoint}");
        byte[] buffer = new byte[1024];
        try
        {
            _stream.BeginRead(buffer, 0, buffer.Length, ReadCallBack, buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting to read from client: {ex.Message}");
            CloseConnection();
        }
    }

    private void ReadCallBack(IAsyncResult ar)
    {
        if (ar.AsyncState is byte[] buffer)
        {
            try
            {
                int bytesRead = _stream.EndRead(ar);
                if (bytesRead > 0)
                {
                    HandleReceivedData(buffer, bytesRead, _connectionId);
                    _stream.BeginRead(buffer, 0, buffer.Length, ReadCallBack, buffer);
                }
                else
                {
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during read from client: {ex.Message}");
                CloseConnection();
            }
        }
        else
        {
            Console.WriteLine("AsyncState is not a byte array or is null.");
            CloseConnection();
        }
    }

    /// <summary>
    /// Processes the received data from a connected client.
    /// </summary>
    /// <param name="data">The received data as a byte array.</param>
    /// <param name="bytesRead">The number of bytes actually read from the network stream.</param>
    private void HandleReceivedData(byte[] data, int bytesRead, string connectionId)
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
            InvokeHandlerForOperationType(operationType, data, connectionId);
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
    private void InvokeHandlerForOperationType(OperationType operationType, byte[] data, string connectionId)
    {
        // Attempt to find the handler for the given operation type in the operationHandlers dictionary.
        var handler = _operationHandlers?.TryGetValue(operationType, out var tempHandler) == true ? tempHandler : null;

        if (handler != null)
        {
            try
            {
                // Call the handler with the received data and byte count.
                handler(_stream, data, connectionId);
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
        _connectionManager.RemoveConnection(_connectionId);
    }
}
