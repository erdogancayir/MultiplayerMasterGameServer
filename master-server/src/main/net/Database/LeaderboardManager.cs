using System.Net.Sockets;
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
            .SetOnInsert(le => le.PlayerID, entry.PlayerID);

        var options = new UpdateOptions { IsUpsert = true };
        await _leaderboard.UpdateOneAsync(filter, update, options);
    }

    public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(int limit)
    {
        return await _leaderboard.Find(_ => true).SortByDescending(le => le.TotalPoints).Limit(limit).ToListAsync();
    }

    // Additional methods as needed
}
