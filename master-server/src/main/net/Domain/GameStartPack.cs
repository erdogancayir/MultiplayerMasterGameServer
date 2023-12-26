using MessagePack;

/* [MessagePackObject]
public class GameStartResponse : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }

    [Key(2)]
    public string? LobbyID { get; set; }

    [Key(3)]
    public int PlayerCount { get; set; }

} */

[MessagePackObject]
public class GameStartResponse : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }
    [Key(2)]
    public int LobbyID { get; set; }
    [Key(3)]
    public int OperationTypeId { get; set; }
    [Key(4)]
    public int PlayerCount { get; set; }
    [Key(5)]
    public List<PlayerInfo> PlayersInLobby { get; set; }
}

public class PlayerInfo
{
    public int PlayerID { get; set; }
    public string Username { get; set; }
}