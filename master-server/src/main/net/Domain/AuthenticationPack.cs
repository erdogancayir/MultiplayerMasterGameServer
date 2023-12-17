using MessagePack;

[MessagePackObject]
public class AuthenticationRequest : BasePack
{
    [Key(1)]
    public string? Username { get; set; }

    [Key(2)]
    public string? Password { get; set; } // Consider hashing the password.
}

[MessagePackObject]
public class AuthenticationResponse : BasePack
{
    [Key(1)]
    public bool IsAuthenticated { get; set; }

    [Key(2)]
    public string? Token { get; set; } // Session token if authentication is successful.

    [Key(3)]
    public string? ErrorMessage { get; set; } // Error message if authentication fails.
}
