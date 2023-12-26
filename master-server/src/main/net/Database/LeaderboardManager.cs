using System.Net.Sockets;
using MessagePack;
using MongoDB.Driver;

public class LeaderboardManager
{
    private readonly IMongoCollection<LeaderboardEntry> _leaderboard;

    public LeaderboardManager(DbInterface dbInterface)
    {
        _leaderboard = dbInterface.GetCollection<LeaderboardEntry>("Leaderboard");
    }

    public async void HandleGetTopLeaderboardEntriesRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
        try
        {
            var request = MessagePackSerializer.Deserialize<GetTopLeaderboardPack>(data);
            int limit = request.TopLimit;

            var topEntriesResponsePacks = await GetTopPlayersAsync(limit);
            var response = new SendTopLeaderboardResponsePack()
            {
                OperationTypeId = (int)OperationType.GetTopLeaderboardEntriesResponse,
                TopEntries = topEntriesResponsePacks
            };

            foreach (var topEntry in topEntriesResponsePacks)
            {
                Console.WriteLine($"Username: {topEntry.Username}, TotalPoints: {topEntry.TotalPoints}");
            }

            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing GetTopLeaderboardEntriesRequest: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates or inserts a leaderboard entry asynchronously.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public async Task UpdateOrInsertLeaderboardEntryAsync(LeaderboardEntry entry)
    {
        var filter = Builders<LeaderboardEntry>.Filter.Eq(le => le.PlayerID, entry.PlayerID);

        var update = Builders<LeaderboardEntry>.Update
            .Inc(le => le.TotalPoints, entry.TotalPoints)
            .Set(le => le.Username, entry.Username)
            .SetOnInsert(le => le.PlayerID, entry.PlayerID);

        var options = new UpdateOptions { IsUpsert = true };
        await _leaderboard.UpdateOneAsync(filter, update, options);
    }

    public async Task<List<GetTopLeaderboardResponsePack>> GetTopPlayersAsync(int limit)
    {
        var leaderboardEntries = await _leaderboard.Find(_ => true)
                                                   .SortByDescending(le => le.TotalPoints)
                                                   .Limit(limit)
                                                   .ToListAsync();

        var responsePacks = leaderboardEntries.Select(le => new GetTopLeaderboardResponsePack
        {
            OperationTypeId = (int)OperationType.GetTopLeaderboardEntriesResponse,
            Username = le.Username,
            TotalPoints = le.TotalPoints,
        }).ToList();

        return responsePacks;
    }
}
