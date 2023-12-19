using MessagePack;

[MessagePackObject]
public class HeartbeatData
{
    [Key(0)]
    public string PlayerId { get; set; }

    [Key(1)]
    public DateTime Timestamp { get; set; }
}