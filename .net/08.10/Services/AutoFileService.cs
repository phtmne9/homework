using System.Globalization;
using System.Text;
using AutoFilesApi.Data;
using AutoFilesApi.Models;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace AutoFilesApi.Services;

public class AutoFileService : IAutoFileService
{
    private static readonly string[] Headers = ["Name", "Producer", "Color", "Cost"];

    private static readonly CsvConfiguration CsvCfg =
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            ReadingExceptionOccurred = _ => false,
        };

    private readonly IAutoRepo _repo;

    public AutoFileService(IAutoRepo repo) => _repo = repo;

    public async Task<byte[]> ExportCsvAsync(CancellationToken ct)
    {
        var rows = await _repo.GetAllAsync(ct);

        using var ms = new MemoryStream();

        await using (var writer = new StreamWriter(ms, new UTF8Encoding(true)))
        await using (var csv = new CsvWriter(writer, CsvCfg))
        {
            await csv.WriteRecordsAsync(rows, ct);
        }

        return ms.ToArray();
    }

    public async Task<byte[]> ExportXlsxAsync(CancellationToken ct)
    {
        var rows = await _repo.GetAllAsync(ct);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Auto");

        for (var i = 0; i < Headers.Length; i++)
            ws.Cell(1, i + 1).Value = Headers[i];

        ws.Row(1).Style.Font.Bold = true;

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var row = i + 2;

            ws.Cell(row, 1).Value = r.Name;
            ws.Cell(row, 2).Value = r.Producer;
            ws.Cell(row, 3).Value = r.Color;
            ws.Cell(row, 4).Value = r.Cost;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        return ms.ToArray();
    }

    public async Task<int> ImportCsvAsync(Stream file, CancellationToken ct)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CsvCfg);

        var rows = new List<AutoRow>();

        await foreach (var row in csv.GetRecordsAsync<AutoRow>(ct))
        {
            if (IsValid(row))
                rows.Add(row);
        }

        return rows.Count == 0 ? 0 : await _repo.AddAsync(rows, ct);
    }

    public async Task<int> ImportXlsxAsync(Stream file, CancellationToken ct)
    {
        using var wb = new XLWorkbook(file);
        var ws = wb.Worksheet(1);

        var rows = new List<AutoRow>();

        foreach (var cells in ws.RowsUsed().Skip(1))
        {
            if (!cells.Cell(4).TryGetValue(out decimal cost))
                continue;

            var row = new AutoRow
            {
                Name = cells.Cell(1).GetString().Trim(),
                Producer = cells.Cell(2).GetString().Trim(),
                Color = cells.Cell(3).GetString().Trim(),
                Cost = cost,
            };

            if (IsValid(row))
                rows.Add(row);
        }

        return rows.Count == 0 ? 0 : await _repo.AddAsync(rows, ct);
    }

    private static bool IsValid(AutoRow r)
        => r.Name.Length > 0 && r.Producer.Length > 0 && r.Cost >= 0;
}
