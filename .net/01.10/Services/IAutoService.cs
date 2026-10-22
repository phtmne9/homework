using AutoApi.Dtos;

namespace AutoApi.Services;

public interface IAutoService
{
    Task<IReadOnlyList<AutoDto>> GetAllAsync(CancellationToken ct);

    Task<AutoDto?> GetAsync(int id, CancellationToken ct);

    Task<AutoDto> CreateAsync(AutoInput input, CancellationToken ct);

    Task<bool> UpdateAsync(int id, AutoInput input, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
