using MessagePack;

[MessagePackObject]
public class GamePack : BasePack
{
    [Key(1)]
    public string? GameID { get; set; }

    [Key(2)]
    public string? LobbyID { get; set; }

    [Key(3)]
    public DateTime StartTime { get; set; }

    [Key(4)]
    public DateTime EndTime { get; set; }

    [Key(5)]
    public Game.GameStatus Status { get; set; }
}
