using MessagePack;

[MessagePackObject]
public class PlayerPositionUpdate : BasePack
{
    [Key(1)]
    public string PlayerId { get; set; } = string.Empty;

    [Key(2)]
    public float X { get; set; }

    [Key(3)]
    public float Y { get; set; }
}