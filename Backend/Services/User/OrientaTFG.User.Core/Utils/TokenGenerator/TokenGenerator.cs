using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrientaTFG.User.Core.Utils.TokenGenerator;

public class TokenGenerator : ITokenGenerator
{
    /// <summary>
    /// Generates the token
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="userRole">The user's role</param>
    /// <param name="secretKey">The secret key</param>
    /// <param name="expiryMinutes">The token expiration minutes</param>
    /// <returns>The generated token</returns>
    public string Generate(string userId, string userRole, string secretKey, int expiryMinutes = 60)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, userRole)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
