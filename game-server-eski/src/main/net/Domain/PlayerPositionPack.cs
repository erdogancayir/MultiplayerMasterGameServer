using MessagePack;

[MessagePackObject]
public class PlayerPositionUpdate : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }

    [Key(2)]
    public float X { get; set; }

    [Key(3)]
    public float Y { get; set; }
}