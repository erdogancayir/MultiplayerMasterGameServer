using MessagePack;

[MessagePackObject]
public class MatchmakingRequest : BasePack
{
    [Key(1)]
    public string Token { get; set; } = string.Empty;
}

[MessagePackObject]
public class MatchmakingResponse : BasePack
{
    [Key(1)]
    public string? LobbyID { get; set; }

    [Key(2)]
    public int PlayerID { get; set; }

    [Key(3)]
    public string? Status { get; set; } // "Waiting", "Full"

    [Key(4)]
    public bool Success { get; set; }

    [Key(5)]
    public string? ErrorMessage { get; set; }
}
