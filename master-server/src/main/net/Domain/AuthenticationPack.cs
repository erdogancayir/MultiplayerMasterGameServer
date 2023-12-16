using MessagePack;

[MessagePackObject]
public class AuthenticationRequest
{
    [Key(0)]
    public string Username { get; set; }

    [Key(1)]
    public string Password { get; set; } // Consider hashing the password.
}

[MessagePackObject]
public class AuthenticationResponse
{
    [Key(0)]
    public bool IsAuthenticated { get; set; }

    [Key(1)]
    public string Token { get; set; } // Session token if authentication is successful.

    [Key(2)]
    public string ErrorMessage { get; set; } // Error message if authentication fails.
}
