using MessagePack;

[MessagePackObject]
public class GameStateMessage
{
    [Key(0)]
    public string GameID { get; set; }

    [Key(1)]
    public string GameStateData { get; set; } // Serialized game state data.
}
