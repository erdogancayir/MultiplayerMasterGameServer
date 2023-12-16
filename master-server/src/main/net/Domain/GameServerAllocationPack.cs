using MessagePack;

[MessagePackObject]
public class ServerAllocationRequest
{
    [Key(0)]
    public string PlayerID { get; set; }
}

[MessagePackObject]
public class ServerAllocationResponse
{
    [Key(0)]
    public string ServerIP { get; set; }

    [Key(1)]
    public int ServerPort { get; set; }
}
