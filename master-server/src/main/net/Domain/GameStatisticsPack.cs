using MessagePack;

[MessagePackObject]
public class GameStatisticsPack : BasePack
{
    [Key(1)]
    public string? GameStatisticsID { get; set; }

    [Key(2)]
    public string? GameID { get; set; }

    [Key(3)]
    public string? PlayerID { get; set; }

    [Key(4)]
    public int Rank { get; set; }

    [Key(5)]
    public int Points { get; set; }
}
