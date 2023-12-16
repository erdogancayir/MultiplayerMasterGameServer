using MessagePack;

[MessagePackObject]
public class LobbyStatusUpdate
{
    [Key(0)]
    public string LobbyID { get; set; }

    [Key(1)]
    public List<string> PlayerIDs { get; set; }

    [Key(2)]
    public bool IsGameStarting { get; set; }

    [Key(3)]
    public int Countdown { get; set; } // Time until the game starts.
}
