using MessagePack;

[MessagePackObject]
public class MatchmakingRequest : BasePack
{
    [Key(1)]
    public string Token { get; set; } = string.Empty;

    [Key(2)]
    public string LobbyID { get; set; } = string.Empty;
}

[MessagePackObject]
public class MatchmakingResponse : BasePack
{
    [Key(1)]
    public bool Success { get; set; } = false;
    
    [Key(2)]
    public string LobbyID { get; set; } = string.Empty;

    [Key(3)]
    public List<string>? PlayerIDs { get; set; }
}
