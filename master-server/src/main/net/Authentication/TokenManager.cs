using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


public class TokenManager
{
    private readonly string _secretKey; // Key used for JWT token generation

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenManager"/> class.
    /// </summary>
    /// <param name="tokenStorage">The token storage.</param>
    /// <param name="secretKey">The secret key used for JWT token generation.</param>
    public TokenManager(string secretKey)
    {
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

        return tokenString;
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The player ID if valid; otherwise, null.</returns>
    public string? ValidateToken(string token)
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

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            Claim playerIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ?? throw new Exception("Player ID claim not found.");

            return playerIdClaim?.Value;
        }
        catch
        {
            return null;
        }
    }
}
