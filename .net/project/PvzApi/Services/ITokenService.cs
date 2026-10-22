using PvzApi.Entities;
using PvzApi.Models;

namespace PvzApi.Services;

public interface ITokenService
{
    TokenResponse Create(User user);
}
