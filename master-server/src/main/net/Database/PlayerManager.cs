using MongoDB.Driver;
using System.Threading.Tasks;

public class PlayerManager
{
    private readonly IMongoCollection<Player> _players;

    /// <summary>
    /// Initializes a new instance of the PlayerManager class.
    /// </summary>
    /// <param name="dbInterface">The database interface for accessing the players collection.</param>
    public PlayerManager(DbInterface dbInterface)
    {
        _players = dbInterface.GetCollection<Player>("Players");
    }

    /// <summary>
    /// Retrieves a player by their unique player ID.
    /// </summary>
    /// <param name="playerId">The unique identifier for the player.</param>
    /// <returns>The player object if found; otherwise, null.</returns>
    public async Task<Player> GetPlayerById(string playerId)
    {
        return await _players.Find(p => p.PlayerID == playerId).FirstOrDefaultAsync();
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
            Username = username,
            PasswordHash = HashPassword(password),
            RegistrationDate = DateTime.UtcNow, 
            LastLoginDate = DateTime.UtcNow
            // Initialize other fields as necessary
        };

        try
        {
            await _players.InsertOneAsync(player);
            return true;
        }
        catch (Exception ex)
        {
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
}
