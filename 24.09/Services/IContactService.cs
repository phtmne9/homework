using ContactApi.Entities;

namespace ContactApi.Services;

public interface IContactService
{
    Task<IReadOnlyList<Contact>> GetAllAsync();

    Task<int> UploadAsync(Stream file, CancellationToken ct);

    Task<byte[]> ExportAsync();
}
