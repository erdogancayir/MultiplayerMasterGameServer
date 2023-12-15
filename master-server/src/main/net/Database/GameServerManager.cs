using MongoDB.Driver;
using System.Threading.Tasks;

public class GameServerManager
{
    private readonly IMongoCollection<GameServer> _gameServers;

    public GameServerManager(DbInterface dbInterface)
    {
        _gameServers = dbInterface.GetCollection<GameServer>("GameServers");
    }

    public async Task AddGameServer(GameServer server)
    {
        await _gameServers.InsertOneAsync(server);
    }

    // ... Additional methods for updating, finding, and deleting game servers ...
}

public class GameServer
{
    public string ServerID { get; set; }
    // ... Other fields ...
}
