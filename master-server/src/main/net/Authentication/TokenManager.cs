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

    public TokenManager(ITokenStorage tokenStorage, string secretKey)
    {
        _tokenStorage = tokenStorage;
        _secretKey = secretKey;
    }

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

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return _tokenStorage.IsTokenValid(token);
        }
        catch
        {
            return false;
        }
    }

    public void InvalidateToken(string playerId)
    {
        _tokenStorage.RemoveToken(playerId);
    }
}
