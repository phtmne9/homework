using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using PagingApi.Models;

namespace PagingApi.Data;

public class AutoRepo : IAutoRepo
{
    private readonly string _cs;

    public AutoRepo(IConfiguration cfg) => _cs = cfg.GetConnectionString("Default")!;

    public async Task<Paged<Auto>> GetPageAsync(
        int page, int size, string? producer, CancellationToken ct)
    {
        var cmd = new CommandDefinition(
            "dbo.usp_AutoPage",
            new { Page = page, PageSize = size, Producer = producer },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        await using var db = new SqlConnection(_cs);

        using var rs = await db.QueryMultipleAsync(cmd);

        var items = (await rs.ReadAsync<Auto>()).ToList();
        var total = await rs.ReadSingleAsync<int>();

        return new Paged<Auto>(items, total, page, size);
    }
}
