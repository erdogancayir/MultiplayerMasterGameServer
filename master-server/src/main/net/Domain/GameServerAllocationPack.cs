using MessagePack;

[MessagePackObject]
public class ServerAllocationRequest : BasePack
{
    [Key(1)]
    public int? PlayerID { get; set; }
}

[MessagePackObject]
public class ServerAllocationResponse : BasePack
{
    [Key(1)]
    public string? ServerIP { get; set; }

    [Key(2)]
    public int ServerPort { get; set; }
}
