using MongoDB.Driver;

public class LobbyManager
{
    private readonly IMongoCollection<Lobby> _lobbies;

    public LobbyManager(DbInterface dbInterface)
    {
        _lobbies = dbInterface.GetCollection<Lobby>("Lobbies");
    }

    /// <summary>
    /// Creates a new lobby in the database.
    /// </summary>
    /// <param name="lobby">The lobby object to be inserted.</param>
    public async Task CreateLobby(Lobby lobby)
    {
        await _lobbies.InsertOneAsync(lobby);
    }

    /// <summary>
    /// Updates the status of a specific lobby.
    /// </summary>
    /// <param name="lobbyID">The unique identifier of the lobby to be updated.</param>
    /// <param name="newStatus">The new status to set for the lobby.</param>
    public async Task UpdateLobby(string lobbyID, Lobby.LobbyStatus newStatus)
    {
        var filter = Builders<Lobby>.Filter.Eq(l => l.LobbyID, lobbyID);
        var update = Builders<Lobby>.Update.Set(l => l.Status, newStatus);
        await _lobbies.UpdateOneAsync(filter, update);
    }


    /// <summary>
    /// Finds a lobby by its identifier.
    /// </summary>
    /// <param name="lobbyID">The unique identifier of the lobby to find.</param>
    /// <returns>The Lobby object if found, otherwise null.</returns>
    public async Task<Lobby> FindLobby(string lobbyID)
    {
        return await _lobbies.Find(l => l.LobbyID == lobbyID).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Deletes a lobby from the database.
    /// </summary>
    /// <param name="lobbyID">The unique identifier of the lobby to be deleted.</param>
    public async Task DeleteLobby(string lobbyID)
    {
        await _lobbies.DeleteOneAsync(l => l.LobbyID == lobbyID);
    }

    /// <summary>
    /// Retrieves all lobbies from the database.
    /// </summary>
    /// <returns>A list of all lobbies.</returns>
    public async Task<List<Lobby>> GetLobbies()
    {
        return await _lobbies.Find(l => true).ToListAsync();
    }

    /// <summary>
    /// Updates the list of players in a specific lobby.
    /// </summary>
    /// <param name="lobbyID">The unique identifier of the lobby to be updated.</param>
    /// <param name="players">The new list of players for the lobby.</param>
    public async Task UpdateLobbyPlayers(string? lobbyID, List<int> players)
    {
        var filter = Builders<Lobby>.Filter.Eq(l => l.LobbyID, lobbyID);
        var update = Builders<Lobby>.Update.Set(l => l.Players, players);
        await _lobbies.UpdateOneAsync(filter, update);
    }
}
