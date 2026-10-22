using Dapper;
using JwtApi.Models;
using Microsoft.Data.Sqlite;

namespace JwtApi.Data;

public class UserRepo : IUserRepo
{
    private static readonly User[] Seed =
    [
        new()
        {
            Login = "admin",
            PasswordHash =
                "100000.LyJ5YKILTMLJxM+V4pkTeYjFvLmAYgW6NK4xrG81UK8="
                + ".gIH/Una4WLF1XgCBsjg4ooJ6CwHnJhBijLykQlHGTAc=",
            Role = "Admin",
            Department = "IT",
        },
        new()
        {
            Login = "manager",
            PasswordHash =
                "100000.2JoQFHUUlgQTS0/HqImwTB+59CvakX4kfmCmqxJY8W4="
                + ".lTapCfnkje7I/5iZvpNMUWwn3PuD6DXZyeULkZB3pqY=",
            Role = "Manager",
            Department = "Sales",
        },
        new()
        {
            Login = "user",
            PasswordHash =
                "100000.igltqQeyJrWryLYcsFswwKUHycQJKxGOoGoiz499r+s="
                + ".EMCn0l7jXkhyHCaL2kpYyYFmhyWriHMGhwSEurEz04k=",
            Role = "User",
            Department = "Support",
        },
    ];

    private readonly string _cs;

    public UserRepo(IConfiguration cfg) => _cs = cfg.GetConnectionString("Default")!;

    public async Task InitAsync()
    {
        const string schema =
            """
            create table if not exists Users (
                Id           integer primary key autoincrement,
                Login        text not null unique,
                PasswordHash text not null,
                Role         text not null,
                Department   text not null
            )
            """;

        const string insert =
            """
            insert into Users (Login, PasswordHash, Role, Department)
            values (@Login, @PasswordHash, @Role, @Department)
            """;

        await using var db = new SqliteConnection(_cs);

        await db.ExecuteAsync(schema);

        if (await db.ExecuteScalarAsync<int>("select count(*) from Users") > 0)
            return;

        await db.ExecuteAsync(insert, Seed);
    }

    public async Task<User?> FindAsync(string login, CancellationToken ct)
    {
        const string sql =
            """
            select Id, Login, PasswordHash, Role, Department
            from Users
            where Login = @login
            """;

        var cmd = new CommandDefinition(sql, new { login }, cancellationToken: ct);

        await using var db = new SqliteConnection(_cs);

        return await db.QuerySingleOrDefaultAsync<User>(cmd);
    }
}
