using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


/// <summary>
/// In-memory implementation of <see cref="ITokenStorage"/>.
/// </summary>
public class InMemoryTokenStorage
{
    private readonly Dictionary<string, string> _tokens = new Dictionary<string, string>();

    /// <inheritdoc />
    public void StoreToken(string playerId, string token)
    {
        _tokens[playerId] = token;
    }

    /// <inheritdoc />
    public bool IsTokenValid(string token)
    {
        Console.WriteLine($"token   :  {token}" );
        foreach(var item in _tokens)
        {
            Console.WriteLine($"item 1 {item}");
        }
        return _tokens.ContainsValue(token);
    }

    /// <inheritdoc />
    public void RemoveToken(string playerId)
    {
        _tokens.Remove(playerId);
    }

    public string? GetPlayerIdForToken(string token)
    {
        foreach (var kvp in _tokens)
        {
            if (kvp.Value == token)
            {
                return kvp.Key;
            }
        }
        return null;
    }
}
