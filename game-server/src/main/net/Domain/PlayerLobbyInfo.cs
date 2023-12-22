
using MessagePack;

[MessagePackObject]
public class PlayerLobbyInfo : BasePack
{
    [Key(1)]
    public int PlayerId { get; set; }

    [Key(2)]
    public string? LobbyId { get; set; }
}
