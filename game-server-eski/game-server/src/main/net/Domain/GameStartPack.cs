using MessagePack;

[MessagePackObject]
public class GameStartResponese : BasePack
{
    [Key(1)]
    public string? LobbyID { get; set; }

    [Key(2)]
    public int PlayerCount { get; set; }

}