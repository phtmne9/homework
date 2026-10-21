namespace CityApi.Services;

public interface ICityFileService
{
    Task<int> UploadAsync(Stream file, CancellationToken ct);

    Task<byte[]> DownloadAsync(CancellationToken ct);
}
