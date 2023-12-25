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
    public List<LeaderboardEntry>? LeaderboardEntries { get; set; }
}