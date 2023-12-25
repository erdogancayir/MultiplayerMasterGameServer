using MessagePack;

[MessagePackObject]
public class GameSavePack : BasePack
{
    [Key(1)]
    public Game? GameData { get; set; }

    [Key(2)]
    public int PlayerID { get; set; }
}

[MessagePackObject]
public class GameSaveResponsePack : BasePack
{
    [Key(1)]
    public bool Success { get; set; }
}