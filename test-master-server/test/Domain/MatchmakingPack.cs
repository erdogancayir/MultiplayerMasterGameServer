using MessagePack;

[MessagePackObject]
public class MatchmakingRequest : BasePack
{
    [Key(1)]
    public string PlayerID { get; set; }

    [Key(2)]
    public string LobbyID { get; set; }
}

[MessagePackObject]
public class MatchmakingResponse : BasePack
{
    [Key(1)]
    public string LobbyID { get; set; }

    [Key(2)]
    public List<string> PlayerIDs { get; set; }
}
