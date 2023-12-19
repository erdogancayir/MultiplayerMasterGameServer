using MessagePack;

[MessagePackObject]
public class BasePack
{
    [Key(0)]
    public int OperationTypeId { get; set; }
}