using MessagePack;

[MessagePackObject]
public class HeartbeatMessage : BasePack
{
    [Key(1)]
    public string ServerID { get; set; }

    [Key(2)]
    public DateTime Timestamp { get; set; }
}
