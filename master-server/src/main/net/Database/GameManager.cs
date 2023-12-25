using System.Net.Sockets;
using MessagePack;
using MongoDB.Driver;

public class GameManager
{
    private readonly IMongoCollection<Game> _games;
    private readonly LeaderboardManager _leaderboardManager;
    private readonly LobbyManager _lobbyManager;

    public GameManager(DbInterface dbInterface, LeaderboardManager leaderboardManager, LobbyManager lobbyManager)
    {
        _games = dbInterface.GetCollection<Game>("Games");
        _leaderboardManager = leaderboardManager;
        _lobbyManager = lobbyManager;
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
                response.Success = false;
                await clientStream.WriteAsync(MessagePackSerializer.Serialize(response), 0, 1);
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
            await _lobbyManager.DeleteLobbyAsync(game.LobbyID);
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
        Console.WriteLine("GetGameRequest received.");
        try
        {
        var getGameRequest = MessagePackSerializer.Deserialize<GetGamePack>(data);
            var response = new GetGameResponsePack
            {
                OperationTypeId = (int)OperationType.GetGameResponsePack
            };

            if (getGameRequest == null || getGameRequest.LobbyId == null)
            {
                response.Success = false;
                await clientStream.WriteAsync(MessagePackSerializer.Serialize(response));
                Console.WriteLine("GetGameRequest is null or invalid.");
                return;
            }

            var game = await GetGameAsyncByLobbyId(getGameRequest.LobbyId);
            if (game != null)
            {
                response.Success = true;
                response.GameData = game;
            }
            else
            {
                response.Success = false; // No game found for the given lobbyId
            }

            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing GetGameRequest: {ex.Message}");
        }
    }

    private async Task<Game?> GetGameAsyncByLobbyId(string lobbyId)
    {
        // Define the filter to search for the game by lobbyId
        var filter = Builders<Game>.Filter.Eq(g => g.LobbyID, lobbyId);

        // Use the Find method to search the collection and retrieve the game
        // Assuming that there's only one game per lobbyId or you're interested in the first one
        var game = await _games.Find(filter).FirstOrDefaultAsync();
        return game; 
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
