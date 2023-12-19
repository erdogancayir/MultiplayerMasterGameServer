using MessagePack;

[MessagePackObject]
public class GameStateMessage : BasePack
{
    [Key(1)]
    public string GameID { get; set; }

    [Key(2)]
    public string GameStateData { get; set; } // Serialized game state data.
}
