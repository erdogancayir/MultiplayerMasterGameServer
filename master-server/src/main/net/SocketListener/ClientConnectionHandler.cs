using System.Net.Sockets;

public class ClientConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly Dictionary<OperationType, Action<NetworkStream, byte[], int>>? _operationHandlers;

    public ClientConnectionHandler(TcpClient client, Dictionary<OperationType, Action<NetworkStream, byte[], int>> operationHandlers)    {
        _client = client;
        _stream = client.GetStream();
    }

    public async Task HandleConnectionAsync()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            
            if (bytesRead > 0)
            {
                // Burada gelen veriyi işleyin.
                // Örneğin, HandleReceivedData metodunu çağırabilirsiniz.
                HandleReceivedData(buffer, bytesRead);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client connection: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }

    private void HandleReceivedData(byte[] data, int bytesRead)
    {
        // Veri işleme işlemleri...
        // Burada gelen veriyi işleyin ve gerekirse _stream üzerinden yanıt gönderin.
    }

    private void CloseConnection()
    {
        _stream.Close();
        _client.Close();
    }
}
