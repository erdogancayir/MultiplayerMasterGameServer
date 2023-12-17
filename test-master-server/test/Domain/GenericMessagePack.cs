using MessagePack;

[MessagePackObject]
public class GenericMessagePacket<T>
{
    [Key(0)]
    public OperationType OperationType { get; set; }

    [Key(1)]
    public T Data { get; set; }
}
