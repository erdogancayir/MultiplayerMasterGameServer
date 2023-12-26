using MessagePack;

[MessagePackObject]
public class GameEndInfoPack : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }
    [Key(2)]
    public string? Username { get; set; }
}