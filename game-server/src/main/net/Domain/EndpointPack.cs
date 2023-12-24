using MessagePack;

[MessagePackObject]
public class EndPointPack : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }
}