using MessagePack;

[MessagePackObject]
public class GetTopLeaderboardPack : BasePack
{
    [Key(1)]
    public int TopLimit { get; set; } = 10;
}

public class GetTopLeaderboardResponsePack : BasePack
{
    [Key(1)]
    public string? Username { get; set; }
    [Key(2)]
    public int TotalPoints { get; set; }
}

public class SendTopLeaderboardResponsePack : BasePack
{
    [Key(1)]
    public List<GetTopLeaderboardResponsePack>? TopEntries { get; set; }
}