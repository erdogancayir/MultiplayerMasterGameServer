using MessagePack;

[MessagePackObject]
public class GenericPacket
{
    [Key(0)]
    public OperationType OperationType { get; set; }

}