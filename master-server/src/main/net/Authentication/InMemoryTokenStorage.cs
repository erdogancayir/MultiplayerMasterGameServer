/// <summary>
/// In-memory implementation of <see cref="ITokenStorage"/>.
/// </summary>
public class InMemoryTokenStorage : ITokenStorage
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
        return _tokens.ContainsValue(token);
    }

    /// <inheritdoc />
    public void RemoveToken(string playerId)
    {
        _tokens.Remove(playerId);
    }
}
