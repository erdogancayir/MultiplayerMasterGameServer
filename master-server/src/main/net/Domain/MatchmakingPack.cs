using MessagePack;

[MessagePackObject]
public class MatchmakingRequest
{
    [Key(0)]
    public string PlayerID { get; set; }

    [Key(1)]
    public string LobbyID { get; set; }
}

[MessagePackObject]
public class MatchmakingResponse
{
    [Key(0)]
    public string LobbyID { get; set; }

    [Key(1)]
    public List<string> PlayerIDs { get; set; }
}
