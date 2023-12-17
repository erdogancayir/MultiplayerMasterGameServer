using MessagePack;

[MessagePackObject]
public class SignUpRequest : BasePack
{
    // Diğer alanlar
    [Key(1)]
    public string Username { get; set; } = "defaultUser";

    [Key(2)]
    public string Email { get; set; } = "user@example.com";

    [Key(3)]
    public string Password { get; set; } = "defaultPassword";
}


[MessagePackObject]
public class SignUpResponse : BasePack
{
    [Key(1)]
    public bool Success { get; set; } = false; // Varsayılan başarı durumu.

    [Key(2)]
    public string Message { get; set; }  = "Operation not completed";
}
