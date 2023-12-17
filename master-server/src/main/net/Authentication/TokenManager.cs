using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public interface ITokenStorage
{
    void StoreToken(string playerId, string token);
    bool IsTokenValid(string token);
    void RemoveToken(string playerId);
    // Diğer metodlar...
}


public class TokenManager
{
    private readonly ITokenStorage _tokenStorage;
    private readonly string _secretKey; // Key used for JWT token generation

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenManager"/> class.
    /// </summary>
    /// <param name="tokenStorage">The token storage.</param>
    /// <param name="secretKey">The secret key used for JWT token generation.</param>
    public TokenManager(ITokenStorage tokenStorage, string secretKey)
    {
        _tokenStorage = tokenStorage;
        _secretKey = secretKey;
    }

    /// <summary>
    /// Generates a JWT token for a player.
    /// </summary>
    /// <param name="player">The player for whom the token is generated.</param>
    /// <returns>A JWT token string.</returns>
    public string GenerateToken(Player player)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] 
            {
                new Claim(ClaimTypes.NameIdentifier, player.PlayerID ?? string.Empty),
            }),
            Expires = DateTime.UtcNow.AddDays(7), // Token expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _tokenStorage.StoreToken(player.PlayerID ?? string.Empty, tokenString);

        return tokenString;
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The player ID if valid; otherwise, null.</returns>
    public string ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);


            var jwtToken = validatedToken as JwtSecurityToken;
            var playerIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            return _tokenStorage.IsTokenValid(token) ? playerIdClaim : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Invalidates a token for a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player whose token is to be invalidated.</param>
    public void InvalidateToken(string playerId)
    {
        _tokenStorage.RemoveToken(playerId);
    }
}
