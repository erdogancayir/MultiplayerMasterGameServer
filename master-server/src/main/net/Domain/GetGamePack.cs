using MessagePack;

[MessagePackObject]
public class GetGamePack : BasePack
{
    [Key(1)]
    public string? LobbyId { get; set; }
}

[MessagePackObject]
public class GetGameResponsePack : BasePack
{
    [Key(1)]
    public bool Success { get; set; }

    [Key(2)]
    public Game? GameData { get; set; }
}