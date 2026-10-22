using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtApi.Services;

public class TokenService : ITokenService
{
    public const string DepartmentClaim = "department";

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
            new(DepartmentClaim, user.Department),
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse(jwt, expires, user.Role, user.Department);
    }
}
