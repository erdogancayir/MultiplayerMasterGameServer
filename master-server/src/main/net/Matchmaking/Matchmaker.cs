using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;
using MongoDB.Driver;

public class Matchmaker
{
    private ConcurrentQueue<string> matchmakingQueue = new ConcurrentQueue<string>();
    private LobbyManager lobbyManager;
    private readonly int requiredPlayersForMatch = 2; // Or any number based on your game's rules
    private readonly TokenManager _tokenManager;
    private PlayerManager playerManager;

    public Matchmaker(LobbyManager lobbyManager, TokenManager tokenManager, PlayerManager playerManager)
    {
        this.lobbyManager = lobbyManager;
        _tokenManager = tokenManager;
        this.playerManager = playerManager;
    }

    public async void HandleJoinLobbyRequest(NetworkStream stream, byte[] data, int length)
    {
        try
        {
            Console.WriteLine("Join lobby request received.");
            var joinLobbyRequest = MessagePackSerializer.Deserialize<MatchmakingRequest>(data);
            var joinLobbyResponse = new MatchmakingResponse();
            joinLobbyResponse.OperationTypeId = (int)OperationType.JoinLobbyResponse;

            // Token doğrulaması
            var playerId = _tokenManager.ValidateToken(joinLobbyRequest.Token);
            if (playerId == null)
            {
                // Hata: Token geçersiz
                var response = new MatchmakingResponse { OperationTypeId = (int)OperationType.JoinLobbyResponse, Success = false };
                await SendMessage(stream, response);
                return;
            }

            await AddPlayerToQueue(playerId);
            await SendMessage(stream, joinLobbyResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling join lobby request: {ex.Message}");
        }
    }

    public async Task AddPlayerToQueue(string playerId)
    {
        matchmakingQueue.Enqueue(playerId);
        await TryCreateMatch(); // Asenkron çağrı
    }


    private async Task TryCreateMatch()
    {
        if (matchmakingQueue.Count >= requiredPlayersForMatch)
        {
            var playerIdsForMatch = new List<string>();

            while (playerIdsForMatch.Count < requiredPlayersForMatch && matchmakingQueue.TryDequeue(out string playerId))
            {
                playerIdsForMatch.Add(playerId);
            }

            if (playerIdsForMatch.Count == requiredPlayersForMatch)
            {
                var playersForMatch = await GetPlayersByIds(playerIdsForMatch);

                var newLobby = new Lobby
                {
                    Players = playerIdsForMatch,
                    Status = "Waiting",
                    CreationTime = DateTime.UtcNow
                };

                await lobbyManager.CreateLobby(newLobby);
                NotifyPlayersAboutLobbyAssignment(playersForMatch, newLobby);
            }
        }
    }

     private async Task<List<Player>> GetPlayersByIds(List<string> playerIds)
    {
        return await playerManager.GetPlayersByIdsAsync(playerIds); // PlayerManager kullanılarak oyuncular çekiliyor
    }


    private void NotifyPlayersAboutLobbyAssignment(List<Player> players, Lobby lobby)
    {
        foreach (var player in players)
        {
            // Implement the logic to notify players
            // This could be a message over a network stream, an event, etc.
            // Example: SendMatchmakingResponse(player, lobby);
        }
    }

    private async Task SendMessage(NetworkStream stream, BasePack response)
    {
        var responseData = MessagePackSerializer.Serialize(response);
        await stream.WriteAsync(responseData, 0, responseData.Length);
    }

    // ... Additional methods and logic ...
}
