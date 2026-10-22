using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PvzApi.Entities;
using PvzApi.Models;

namespace PvzApi.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    public TokenResponse Create(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role),
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            Role = user.Role,
        };
    }
}
