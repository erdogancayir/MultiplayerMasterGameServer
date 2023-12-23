using System.Net.Sockets;
using MessagePack;
using MongoDB.Driver;

public class GameManager
{
    private readonly IMongoCollection<Game> _games;
    private readonly LeaderboardManager _leaderboardManager;

    public GameManager(DbInterface dbInterface, LeaderboardManager leaderboardManager)
    {
        _games = dbInterface.GetCollection<Game>("Games");
        _leaderboardManager = leaderboardManager;
    }

    /// <summary>
    /// Handles a CreateGameRequest.
    /// </summary>
    /// <param name="clientStream"></param>
    /// <param name="data"></param>
    /// <param name="connectionId"></param>
    public async void HandleCreateGameRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
        Console.WriteLine("CreateGameRequest received.");
        try
        {
            var createGameRequest = MessagePackSerializer.Deserialize<GameSavePack>(data);
            var response = new GameSaveResponsePack();
            response.OperationTypeId = (int)OperationType.GameSaveResponsePack;

            if (createGameRequest == null || createGameRequest.GameData == null)
            {
                Console.WriteLine("CreateGameRequest is null.");
                return;
            }
            var game = new Game
            {
                LobbyID = createGameRequest.GameData.LobbyID,
                EndTime = createGameRequest.GameData.EndTime,
                Status = createGameRequest.GameData.Status,
                PlayerID = createGameRequest.PlayerID
            };
            await CreateGameAsync(game);
            response.Success = true;
            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
            await UpdateLeaderboardAfterGame(game);
            Console.WriteLine($"Game {game.GameID} created.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing CreateGameRequest: {ex.Message}");
        }
    }

    public async void HandleGetGameRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
    }

    /// <summary>
    /// Updates the leaderboard after a game has ended.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private async Task UpdateLeaderboardAfterGame(Game game)
    {
        var pointsToAdd = 10;

        var leaderboardEntry = new LeaderboardEntry
        {
            PlayerID = game.PlayerID,
            TotalPoints = pointsToAdd
        };
        await _leaderboardManager.UpdateOrInsertLeaderboardEntryAsync(leaderboardEntry);
    }

    public async Task CreateGameAsync(Game game)
    {
        await _games.InsertOneAsync(game);
    }

    public async Task UpdateGameAsync(string gameId, Game.GameStatus status)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameID, gameId);
        var update = Builders<Game>.Update.Set(g => g.Status, status);
        await _games.UpdateOneAsync(filter, update);
    }

    public async Task<Game> GetGameAsync(string gameId)
    {
        return await _games.Find(g => g.GameID == gameId).FirstOrDefaultAsync();
    }
}
