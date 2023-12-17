using System;
using System.Threading.Tasks;
using MongoDB.Driver;

public class LogManager
{
    private readonly IMongoCollection<LogEntry> _logCollection;

    public LogManager(DbInterface dbInterface)
    {
        _logCollection = dbInterface.GetCollection<LogEntry>("Logs");
    }

    public async Task CreateLogAsync(string type, string message, string? playerId = null, string? serverId = null)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Type = type,
            Message = message,
            PlayerID = playerId,
            ServerID = serverId
        };

        await _logCollection.InsertOneAsync(logEntry);
    }
}
