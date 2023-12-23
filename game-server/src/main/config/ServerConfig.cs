public class ServerConfig
{
    public int MaxPlayersPerLobby { get; set; }
    public int MaxLobbies { get; set; }
    public int SocketTcpListenerPort { get; set; }
    public int SocketUdpListenerPort { get; set; }
    public int MasterServerTcpPort { get; set; }
    public string MasterServerIp { get; set; } = "127.0.0.1";
    public string? JwtSecretKey { get; set; }

}
