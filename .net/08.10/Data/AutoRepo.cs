using AutoFilesApi.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AutoFilesApi.Data;

public class AutoRepo : IAutoRepo
{
    private readonly string _cs;

    public AutoRepo(IConfiguration cfg) => _cs = cfg.GetConnectionString("Default")!;

    public async Task<IReadOnlyList<AutoRow>> GetAllAsync(CancellationToken ct)
    {
        var cmd = new CommandDefinition(
            "select Name, Producer, Color, Cost from dbo.Auto order by Id",
            cancellationToken: ct);

        await using var db = new SqlConnection(_cs);

        var rows = await db.QueryAsync<AutoRow>(cmd);

        return rows.ToList();
    }

    public async Task<int> AddAsync(IEnumerable<AutoRow> rows, CancellationToken ct)
    {
        const string sql =
            """
            insert into dbo.Auto (Name, Producer, Color, Cost)
            values (@Name, @Producer, @Color, @Cost)
            """;

        var cmd = new CommandDefinition(sql, rows, cancellationToken: ct);

        await using var db = new SqlConnection(_cs);

        return await db.ExecuteAsync(cmd);
    }
}
