using PagingApi.Models;

namespace PagingApi.Data;

public interface IAutoRepo
{
    Task<Paged<Auto>> GetPageAsync(int page, int size, string? producer, CancellationToken ct);
}
