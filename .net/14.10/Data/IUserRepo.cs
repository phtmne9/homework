using JwtApi.Models;

namespace JwtApi.Data;

public interface IUserRepo
{
    Task InitAsync();

    Task<User?> FindAsync(string login, CancellationToken ct);
}
