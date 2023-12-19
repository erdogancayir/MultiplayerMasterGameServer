using MessagePack;

[MessagePackObject]
public class PlayerPosition
{
    [Key(0)]
    public string PlayerId { get; set; }

    [Key(1)]
    public float X { get; set; }

    [Key(2)]
    public float Y { get; set; }

    [Key(3)]
    public float Z { get; set; }
}