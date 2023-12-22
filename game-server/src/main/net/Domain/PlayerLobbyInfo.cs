
using MessagePack;

[MessagePackObject]
public class PlayerLobbyInfo : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }

    [Key(2)]
    public int LobbyId { get; set; }

    [Key(3)]
    public float X { get; set; }

    [Key(4)]
    public float Y { get; set; }
}
