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

    /// <summary>
    /// Creates a new log entry in the database.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    /// <param name="playerId"></param>
    /// <param name="serverId"></param>
    /// <returns></returns>
    public async Task CreateLogAsync(string type, string message, int? playerId = null, string? serverId = null)
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
