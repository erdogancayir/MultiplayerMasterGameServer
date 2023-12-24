using System.Net;

public class PlayerData
{
    public int PlayerId { get; set; }
    public string? LobbyId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public IPEndPoint EndPoint { get; set; } = null;
}
