using System.Net;
using System.Net.Sockets;

public class UdpConnectionManager
{
    public string PlayerId { get; set; } = string.Empty;
    public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 0);

    // Oyuncunun diğer bilgileri, örneğin X ve Y pozisyonları
    public float X { get; set; }
    public float Y { get; set; }

    public void SendMessage(byte[] message)
    {
    }
}
