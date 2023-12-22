using MessagePack;

[MessagePackObject]
public class LeaderboardEntryPack : BasePack
{
    [Key(1)]
    public string? LeaderboardEntryID { get; set; }

    [Key(2)]
    public int? PlayerID { get; set; }

    [Key(3)]
    public int TotalPoints { get; set; }
}
