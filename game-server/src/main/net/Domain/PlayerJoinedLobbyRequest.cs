using MessagePack;

[MessagePackObject]
public class PlayerJoinedLobbyRequest : BasePack
{
    [Key(1)]
    public int PlayerID { get; set; }

    [Key(2)]
    public string LobbyID { get; set; } = string.Empty;
}