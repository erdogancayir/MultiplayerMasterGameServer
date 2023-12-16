using MessagePack;
[MessagePackObject]
public class PlayerRequest
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public float x;

    [Key(2)]
    public float y;

    [Key(3)]
    public float z;

    public PlayerRequest()
    {
        Id = 0;
        x = 0;
        y = 0;
        z = 0;
    }

    public PlayerRequest(int id, float x, float y, float z)
    {
        Id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Additional properties and methods...
}
