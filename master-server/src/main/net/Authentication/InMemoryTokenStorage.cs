public class InMemoryTokenStorage : ITokenStorage
{
    private readonly Dictionary<string, string> _tokens = new Dictionary<string, string>();

    public void StoreToken(string playerId, string token)
    {
        _tokens[playerId] = token;
    }

    public bool IsTokenValid(string token)
    {
        return _tokens.ContainsValue(token);
    }

    public void RemoveToken(string playerId)
    {
        _tokens.Remove(playerId);
    }
}
