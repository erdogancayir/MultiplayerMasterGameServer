using MongoDB.Driver;

public class LobbyManager
{
    private readonly IMongoCollection<Lobby> _lobbies;

    public LobbyManager(DbInterface dbInterface)
    {
        _lobbies = dbInterface.GetCollection<Lobby>("Lobbies");
    }

    public async Task CreateLobby(Lobby lobby)
    {
        await _lobbies.InsertOneAsync(lobby);
    }

    public async Task UpdateLobby(string lobbyID, string? newStatus)
    {
        var filter = Builders<Lobby>.Filter.Eq(l => l.LobbyID, lobbyID);
        var update = Builders<Lobby>.Update.Set(l => l.Status, newStatus);
        await _lobbies.UpdateOneAsync(filter, update);
    }

    public async Task<Lobby> FindLobby(string lobbyID)
    {
        return await _lobbies.Find(l => l.LobbyID == lobbyID).FirstOrDefaultAsync();
    }

    public async Task DeleteLobby(string lobbyID)
    {
        await _lobbies.DeleteOneAsync(l => l.LobbyID == lobbyID);
    }

    public async Task<List<Lobby>> GetLobbies()
    {
        return await _lobbies.Find(l => true).ToListAsync();
    }

    public async Task UpdateLobbyPlayers(string lobbyID, List<string> players)
    {
        var filter = Builders<Lobby>.Filter.Eq(l => l.LobbyID, lobbyID);
        var update = Builders<Lobby>.Update.Set(l => l.Players, players);
        await _lobbies.UpdateOneAsync(filter, update);
    }

    // ... Additional methods ...
}
