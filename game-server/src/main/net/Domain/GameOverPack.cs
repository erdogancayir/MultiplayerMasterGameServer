using MessagePack;

[MessagePackObject]
public class GameOverPack : BasePack
{
    [Key(1)]
    public string? LobbyId { get; set; }

    [Key(2)]
    public int WinnerPlayerId { get; set; }  
}

[MessagePackObject]
public class GameOverResponse : BasePack
{
    [Key(1)]
    public bool Success { get; set; }
    [Key(2)]
    public string? LobbyId { get; set; }
}