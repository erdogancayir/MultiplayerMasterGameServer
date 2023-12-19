using MessagePack;

[MessagePackObject]
public class GameSessionInfo
{
    [Key(0)]
    public string SessionId { get; set; }

    [Key(1)]
    public List<string> PlayerIds { get; set; }

    // Additional session info fields as needed
}

[MessagePackObject]
public class GameSessionStartRequest
{
    [Key(0)]
    public string LobbyID { get; set; }

    [Key(1)]
    public List<string> PlayerIDs { get; set; }

    // Add additional game settings as required
}
