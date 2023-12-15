using MongoDB.Driver;
using System.Threading.Tasks;

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

    // ... Additional methods for updating, finding, and deleting lobbies ...
}