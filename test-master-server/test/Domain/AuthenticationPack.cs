using MessagePack;

[MessagePackObject]
public class AuthenticationRequest : BasePack
{
    [Key(1)]
    public string Username { get; set; }

    [Key(2)]
    public string Password { get; set; } // Consider hashing the password.
}

[MessagePackObject]
public class AuthenticationResponse : BasePack
{
    [Key(1)]
    public bool Success { get; set; } = false; // Varsayılan başarı durumu.

    [Key(2)]
    public string? Token { get; set; } // Session token if authentication is successful.

    [Key(3)]
    public string? ErrorMessage { get; set; } // Error message if authentication fails.

    [Key(4)]
    public string Message { get; set; }  = "Operation not completed";
}

[MessagePackObject]
public class LogoutRequest : BasePack
{
    [Key(1)]
    public string? Username { get; set; }
}

[MessagePackObject]
public class LogoutResponse : BasePack
{
    [Key(1)]
    public bool Success { get; set; } = false; // Varsayılan başarı durumu.

    [Key(2)]
    public string? ErrorMessage { get; set; } // Error message if logout fails.

    [Key(3)]
    public string Message { get; set; }  = "Operation not completed";
}
