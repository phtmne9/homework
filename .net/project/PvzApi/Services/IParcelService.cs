using PvzApi.Models;

namespace PvzApi.Services;

public interface IParcelService
{
    Task<PageResult<ParcelDto>> GetPagedAsync(ParcelQuery query, CancellationToken ct = default);

    Task<ParcelDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<ParcelDto?> GetByTrackAsync(string trackNumber, CancellationToken ct = default);

    Task<string> AcceptAsync(ParcelCreateDto parcel, CancellationToken ct = default);

    Task<string> IssueAsync(int id, string phone, CancellationToken ct = default);

    Task<string> ReturnAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ParcelDto>> ExpiringAsync(int days, CancellationToken ct = default);
}
