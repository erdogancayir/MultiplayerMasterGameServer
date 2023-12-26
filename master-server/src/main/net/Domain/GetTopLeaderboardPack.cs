using MessagePack;

[MessagePackObject]
public class GetTopLeaderboardPack : BasePack
{
    [Key(1)]
    public int TopLimit { get; set; }
}

public class GetTopLeaderboardResponsePack : BasePack
{
    [Key(1)]
    public string? LeaderboardEntryID { get; set; }
    [Key(2)]
    public int? PlayerID { get; set; }
    [Key(3)]
    public int TotalPoints { get; set; }
    [Key(4)]
    public string Username { get; set; }
}