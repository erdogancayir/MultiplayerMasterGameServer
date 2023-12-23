using MessagePack;

[MessagePackObject]
public class GameStartResponse : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }

    [Key(2)]
    public string? LobbyID { get; set; }

    [Key(3)]
    public int PlayerCount { get; set; }

}