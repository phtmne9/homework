using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using PvzApi.Services;
using Xunit;

namespace PvzApi.Tests;

public class FileServiceTests
{
    private static FileService Service(TestDb db) =>
        new(db.Context, NullLogger<FileService>.Instance);

    [Fact]
    public async Task ImportCsv_SkipsDuplicatesInsideFile()
    {
        using var db = new TestDb();
        var csv = "FullName;Phone\nПервый;+77010000001\nВторой;+77010000002\nДубль;+77010000001\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = await Service(db).ImportClientsCsvAsync(stream);

        Assert.Equal("Добавлено 2, пропущено 1", result);
    }

    [Fact]
    public async Task ImportCsv_SkipsExistingPhone()
    {
        using var db = new TestDb();
        var phone = db.Context.Clients.First().Phone;
        var csv = $"FullName;Phone\nНовый;{phone}\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = await Service(db).ImportClientsCsvAsync(stream);

        Assert.Equal("Добавлено 0, пропущено 1", result);
    }

    [Fact]
    public async Task ExportCsv_ContainsHeaderAndRow()
    {
        using var db = new TestDb();

        var bytes = await Service(db).ExportParcelsCsvAsync();
        var text = Encoding.UTF8.GetString(bytes);

        Assert.Contains("Track;Client;Phone", text);
        Assert.Contains("PVZ-1", text);
    }

    [Fact]
    public async Task ExportXlsx_ProducesWorkbook()
    {
        using var db = new TestDb();

        var bytes = await Service(db).ExportParcelsXlsxAsync();

        Assert.True(bytes.Length > 1000);
        Assert.Equal(0x50, bytes[0]);
    }
}
