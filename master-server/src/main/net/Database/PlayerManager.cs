using MongoDB.Driver;

public class PlayerManager
{
    private readonly IMongoCollection<Player> _players;
    private readonly LogManager _logManager;

    private readonly TokenManager _tokenManager;

    private readonly DbInterface _dbInterface;

    // <summary>
    /// Initializes a new instance of the PlayerManager class.
    /// </summary>
    /// <param name="dbInterface">The database interface for accessing the players collection.</param>
    /// <param name="logManager">The LogManager for logging activities.</param>
    public PlayerManager(DbInterface dbInterface, LogManager logManager, TokenManager tokenManager)
    {
        _players = dbInterface.GetCollection<Player>("Players");
        _logManager = logManager;
        _tokenManager = tokenManager;
        _dbInterface = dbInterface;
    }

    /// <summary>
    /// Retrieves a player by their unique player ID.
    /// </summary>
    /// <param name="playerId">The unique identifier for the player.</param>
    /// <returns>The player object if found; otherwise, null.</returns>
    public async Task<Player> GetPlayerById(int playerId)
    {
        return await _players.Find(p => p.PlayerID == playerId).FirstOrDefaultAsync();
    }

    public async Task<int> GetPlayerIdByUsername(string username)
    {
        var player = await _players.Find(p => p.Username == username).FirstOrDefaultAsync();
        if (player == null)
        {
            throw new NullReferenceException("Player not found.");
        }

        return Convert.ToInt32(player.PlayerID);
    }


    /// <summary>
    /// Checks if a username is available.
    /// </summary>
    /// <param name="username">The username to check for availability.</param>
    /// <returns>True if the username is available; otherwise, false.</returns>
    public async Task<bool> IsUsernameAvailable(string username)
    {
        var existingPlayer = await _players.Find(p => p.Username == username).FirstOrDefaultAsync();
        return existingPlayer == null; // Returns true if no player found with the given username
    }

    /// <summary>
    /// Creates a new player asynchronously.
    /// </summary>
    /// <param name="username">The username of the new player.</param>
    /// <param name="password">The password of the new player.</param>
    /// <returns>True if the player was successfully created; otherwise, false.</returns>
    public async Task<bool> CreatePlayerAsync(string username, string password)
    {
        var player = new Player
        {
            PlayerID = await GetNextPlayerIdAsync(),
            Username = username,
            PasswordHash = HashPassword(password),
            RegistrationDate = DateTime.UtcNow, 
            LastLoginDate = DateTime.UtcNow
        };

        try
        {
            await _players.InsertOneAsync(player);
            await _logManager.CreateLogAsync("Info", $"New player created: {username}", player.PlayerID);
            return true;
        }
        catch (Exception ex)
        {
            await _logManager.CreateLogAsync("Error", $"Failed to create player: {ex.Message}");
            Console.WriteLine($"Failed to create player: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Retrieves a player by their username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <returns>The player object if found; otherwise, null.</returns>
    public async Task<Player> GetPlayerByUsernameAsync(string? username)
    {
        return await _players.Find(p => p.Username == username).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Validates the provided password against the stored password hash.
    /// </summary>
    /// <param name="storedHash">The stored password hash.</param>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password is valid; otherwise, false.</returns>
    public bool ValidatePassword(string? storedHash, string? password)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }

    /// <summary>
    /// Generates a simple token for authentication purposes.
    /// </summary>
    /// <returns>A newly generated token.</returns>
    public string GenerateToken(Player player)
    {
        return _tokenManager.GenerateToken(player);
    }

    public async Task InvalidateToken(int playerId)
    {
        try
        {
            // Assuming _tokenStorage is an instance of ITokenStorage
            await _logManager.CreateLogAsync("Info", $"Token invalidated for player ID: {playerId}", playerId);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error invalidating token: {ex.Message}");
        }
    }


    public async Task<List<Player>> GetPlayersByIdsAsync(List<int> playerIds)
    {
        var filter = Builders<Player>.Filter.In(p => p.PlayerID, playerIds);
        return await _players.Find(filter).ToListAsync();
    }

    public async Task<Player> GetPlayer(int playerID)
    {
        return await _players.Find(p => p.PlayerID == playerID).FirstOrDefaultAsync();
    }

    public int? PlayerValidateToken(string token)
    {
        try
        {
            var playerId = _tokenManager.ValidateToken(token);
            if (playerId == null || playerId == 0)
            {
                // Geçersiz token durumu
                return null;
            }

            // Geçerli bir playerId varsa, döndür
            return playerId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating token: {ex.Message}");
            return null;
        }
    }

    public async Task InitializeCounterAsync()
    {
        Console.WriteLine("Initializing counters...");
        var counterCollection = _dbInterface.GetCollection<Counter>("Counters");
        var playerCounter = await counterCollection.Find(c => c.Id == "playerId").FirstOrDefaultAsync();
        if (playerCounter == null)
        {
            await counterCollection.InsertOneAsync(new Counter { Id = "playerId", SeqValue = 0 });
        }
    }

    public async Task<int> GetNextPlayerIdAsync()
    {
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, "playerId");
        var update = Builders<Counter>.Update.Inc(c => c.SeqValue, 1);
        var options = new FindOneAndUpdateOptions<Counter, Counter>
        {
            ReturnDocument = ReturnDocument.After
        };

        var counterCollection = _dbInterface.GetCollection<Counter>("Counters");
        var updatedCounter = await counterCollection.FindOneAndUpdateAsync(filter, update, options);

        return updatedCounter.SeqValue;
    }

    public async Task<string> GetUsernameByPlayerId(int playerId)
    {
        var player = await _players.Find(p => p.PlayerID == playerId).FirstOrDefaultAsync();
        if (player == null)
        {
            throw new NullReferenceException("Player not found.");
        }

        return player.Username;
    }
}
