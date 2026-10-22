using JwtApi.Models;

namespace JwtApi.Services;

public interface ITokenService
{
    TokenResponse Create(User user);
}
