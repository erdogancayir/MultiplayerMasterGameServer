using System.Net;
using System.Net.Sockets;

public class UdpConnectionManager
{
    public int LobbyId { get; set; }
    public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 0);
    public float X { get; set; }
    public float Y { get; set; }

    public void SendMessage(byte[] message)
    {
        var udpClient = new UdpClient();
        udpClient.Send(message, message.Length, EndPoint);
    }
}
