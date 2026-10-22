namespace PvzApi.Services;

public interface IFileService
{
    Task<byte[]> ExportParcelsCsvAsync(CancellationToken ct = default);

    Task<byte[]> ExportParcelsXlsxAsync(CancellationToken ct = default);

    Task<string> ImportClientsCsvAsync(Stream file, CancellationToken ct = default);

    Task<string> ImportClientsXlsxAsync(Stream file, CancellationToken ct = default);
}
