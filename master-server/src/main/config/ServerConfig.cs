public class ServerConfig
{
    public int MaxPlayersPerLobby { get; set; }
    public int MaxLobbies { get; set; }
    public int SocketListenerPort { get; set; }

    // Settings for server performance, like thread pool sizes, timeout settings, etc.
}
