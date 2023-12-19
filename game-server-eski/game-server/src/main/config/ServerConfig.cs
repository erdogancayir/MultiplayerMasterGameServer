public class ServerConfig
{
    public int MaxPlayersPerLobby { get; set; }
    public int MaxLobbies { get; set; }
    public int SocketTcpListenerPort { get; set; }
    public int SocketUdpListenerPort { get; set; }

    public string? JwtSecretKey { get; set; }

}
