using MessagePack;

[MessagePackObject]
public class LobbyStatusUpdate : BasePack
{
    [Key(1)]
    public string LobbyID { get; set; }

    [Key(2)]
    public List<string> PlayerIDs { get; set; }

    [Key(3)]
    public bool IsGameStarting { get; set; }

    [Key(4)]
    public int Countdown { get; set; } // Time until the game starts.
}
