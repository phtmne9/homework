using AutoFilesApi.Models;

namespace AutoFilesApi.Data;

public interface IAutoRepo
{
    Task<IReadOnlyList<AutoRow>> GetAllAsync(CancellationToken ct);

    Task<int> AddAsync(IEnumerable<AutoRow> rows, CancellationToken ct);
}
