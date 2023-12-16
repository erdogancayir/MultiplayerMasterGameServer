using MessagePack;

[MessagePackObject]
public class HeartbeatMessage
{
    [Key(0)]
    public string ServerID { get; set; }

    [Key(1)]
    public DateTime Timestamp { get; set; }
}
