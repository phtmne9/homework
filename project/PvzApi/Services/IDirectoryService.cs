using PvzApi.Models;

namespace PvzApi.Services;

public interface IDirectoryService
{
    Task<IReadOnlyList<PickupPointDto>> GetPointsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClientDto>> GetClientsAsync(CancellationToken ct = default);

    Task<string> AddClientAsync(ClientDto client, CancellationToken ct = default);

    Task<string> AddPointAsync(PickupPointDto point, CancellationToken ct = default);
}
