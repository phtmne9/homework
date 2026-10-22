namespace AutoFilesApi.Services;

public interface IAutoFileService
{
    Task<byte[]> ExportCsvAsync(CancellationToken ct);

    Task<byte[]> ExportXlsxAsync(CancellationToken ct);

    Task<int> ImportCsvAsync(Stream file, CancellationToken ct);

    Task<int> ImportXlsxAsync(Stream file, CancellationToken ct);
}
