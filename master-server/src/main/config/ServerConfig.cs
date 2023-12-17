public class ServerConfig
{
    public int MaxPlayersPerLobby { get; set; }
    public int MaxLobbies { get; set; }
    public int SocketListenerPort { get; set; }

    public string? JwtSecretKey { get; set; }

    // Additional server performance settings...
}
