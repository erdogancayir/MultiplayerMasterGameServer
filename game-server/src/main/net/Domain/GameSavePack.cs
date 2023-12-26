using MessagePack;

[MessagePackObject]
public class GameSavePack : BasePack
{
    [Key(1)]
    public string? LobbyID  { get; set; }
    [Key(2)]
    public int PlayerID { get; set; }
    [Key(3)]
    public DateTime EndTime { get; set; }
}

[MessagePackObject]
public class GameSaveResponsePack : BasePack
{
    [Key(1)]
    public bool Success { get; set; }

    [Key(2)]
    public string? LobbyID { get; set; }
}