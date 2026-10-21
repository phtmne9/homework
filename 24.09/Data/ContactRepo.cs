using ContactApi.Entities;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ContactApi.Data;

public class ContactRepo : IContactRepo
{
    private readonly string _cs;

    public ContactRepo(IConfiguration cfg) => _cs = cfg.GetConnectionString("Default")!;

    public async Task InitAsync()
    {
        const string sql =
            """
            create table if not exists Contacts (
                Id       integer primary key autoincrement,
                Name     text not null,
                Phone    text not null,
                AltPhone text not null default '',
                Email    text not null default '',
                Note     text not null default ''
            )
            """;

        await using var db = new SqliteConnection(_cs);

        await db.ExecuteAsync(sql);
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync()
    {
        const string sql =
            "select Id, Name, Phone, AltPhone, Email, Note from Contacts order by Id";

        await using var db = new SqliteConnection(_cs);

        var rows = await db.QueryAsync<Contact>(sql);

        return rows.ToList();
    }

    public async Task<int> AddAsync(IEnumerable<Contact> contacts)
    {
        const string sql =
            """
            insert into Contacts (Name, Phone, AltPhone, Email, Note)
            values (@Name, @Phone, @AltPhone, @Email, @Note)
            """;

        await using var db = new SqliteConnection(_cs);

        return await db.ExecuteAsync(sql, contacts);
    }
}
