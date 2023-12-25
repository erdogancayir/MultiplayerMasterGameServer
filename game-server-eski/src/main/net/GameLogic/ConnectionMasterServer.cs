using System.Net.Sockets;

public class ConnectionMasterServer
{
    private NetworkStream _stream;
    private TcpClient _client;

    // Dışarıdan bir TcpClient ve NetworkStream kabul eden constructor
    public ConnectionMasterServer(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
    }

    public void SetStream(NetworkStream stream)
    {
        _stream = stream;
    }

    public void SendData(byte[] data)
    {
        try
        {
            _stream.Write(data, 0, data.Length);
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error sending data to master server: {e.Message}");
        }
    }

    public byte[]? ReceiveData()
    {
        try
        {
            byte[] data = new byte[1024];
            int bytesRead = _stream.Read(data, 0, data.Length);
            return data;
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error receiving data from master server: {e.Message}");
            return null;
        }
    }
}