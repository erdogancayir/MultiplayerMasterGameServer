using MessagePack;

[MessagePackObject]
public class GameEndDataPack : BasePack
{
    [Key(1)]
    public Game? GameData { get; set; }

    [Key(2)]
    public List<GameStatistic>? GameStatistics { get; set; }

    [Key(3)]
    public List<LeaderboardEntry>? LeaderboardEntries { get; set; }
}
