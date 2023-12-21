using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


/// <summary>
/// In-memory implementation of <see cref="ITokenStorage"/>.
/// </summary>
public class InMemoryTokenStorage
{
    private readonly Dictionary<int, string> _tokens = new Dictionary<int, string>();

    /// <inheritdoc />
    public void StoreToken(int playerId, string token)
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
    public void RemoveToken(int playerId)
    {
        _tokens.Remove(playerId);
    }

    public int? GetPlayerIdForToken(string token)
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
