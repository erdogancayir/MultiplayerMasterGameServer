using MessagePack;

[MessagePackObject]
public class PlayerRequest
{
    [Key(0)]
    public string Username { get; set; }

    [Key(1)]
    public string Action { get; set; }

    // Additional properties as needed
}
