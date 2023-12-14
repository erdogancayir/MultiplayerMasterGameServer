using MessagePack;

[MessagePackObject]
public class MyMessage
{
    [Key(0)]
    public string Content { get; set; }
}
