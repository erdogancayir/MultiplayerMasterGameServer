using System.Net;

public class PlayerData
{
    public int PlayerId { get; set; }
    public int LobbyId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 0);
}
