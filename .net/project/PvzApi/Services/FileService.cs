using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PvzApi.Data;
using PvzApi.Entities;

namespace PvzApi.Services;

public class FileService : IFileService
{
    private readonly AppDbContext _db;
    private readonly ILogger<FileService> _log;

    public FileService(AppDbContext db, ILogger<FileService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<byte[]> ExportParcelsCsvAsync(CancellationToken ct = default)
    {
        var rows = await RowsAsync(ct);

        await using var mem = new MemoryStream();
        await using (var writer = new StreamWriter(mem, new UTF8Encoding(true), leaveOpen: true))
        await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
        }))
        {
            await csv.WriteRecordsAsync(rows, ct);
        }

        return mem.ToArray();
    }

    public async Task<byte[]> ExportParcelsXlsxAsync(CancellationToken ct = default)
    {
        var rows = await RowsAsync(ct);

        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Посылки");

        sheet.Cell(1, 1).Value = "Трек";
        sheet.Cell(1, 2).Value = "Клиент";
        sheet.Cell(1, 3).Value = "Телефон";
        sheet.Cell(1, 4).Value = "ПВЗ";
        sheet.Cell(1, 5).Value = "Статус";
        sheet.Cell(1, 6).Value = "Вес";
        sheet.Cell(1, 7).Value = "Сумма";
        sheet.Cell(1, 8).Value = "Принята";
        sheet.Row(1).Style.Font.Bold = true;

        var line = 2;
        foreach (var r in rows)
        {
            sheet.Cell(line, 1).Value = r.Track;
            sheet.Cell(line, 2).Value = r.Client;
            sheet.Cell(line, 3).Value = r.Phone;
            sheet.Cell(line, 4).Value = r.Point;
            sheet.Cell(line, 5).Value = r.Status;
            sheet.Cell(line, 6).Value = r.Weight;
            sheet.Cell(line, 7).Value = r.Price;
            sheet.Cell(line, 8).Value = r.ArrivedAt;
            line++;
        }

        sheet.Columns().AdjustToContents();

        using var mem = new MemoryStream();
        book.SaveAs(mem);

        return mem.ToArray();
    }

    public async Task<string> ImportClientsCsvAsync(Stream file, CancellationToken ct = default)
    {
        using var reader = new StreamReader(file, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HeaderValidated = null,
            MissingFieldFound = null,
        });

        var added = 0;
        var skipped = 0;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await foreach (var row in csv.GetRecordsAsync<ClientRow>(ct))
        {
            if (string.IsNullOrWhiteSpace(row.Phone) || string.IsNullOrWhiteSpace(row.FullName))
            {
                skipped++;
                continue;
            }

            var phone = row.Phone.Trim();

            if (!seen.Add(phone) || await _db.Clients.AnyAsync(c => c.Phone == phone, ct))
            {
                skipped++;
                continue;
            }

            _db.Clients.Add(new Client { FullName = row.FullName.Trim(), Phone = phone });
            added++;
        }

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Импорт csv: добавлено {Added}, пропущено {Skipped}", added, skipped);

        return $"Добавлено {added}, пропущено {skipped}";
    }

    public async Task<string> ImportClientsXlsxAsync(Stream file, CancellationToken ct = default)
    {
        using var book = new XLWorkbook(file);
        var sheet = book.Worksheets.First();

        var added = 0;
        var skipped = 0;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var name = row.Cell(1).GetString().Trim();
            var phone = row.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                skipped++;
                continue;
            }

            if (!seen.Add(phone) || await _db.Clients.AnyAsync(c => c.Phone == phone, ct))
            {
                skipped++;
                continue;
            }

            _db.Clients.Add(new Client { FullName = name, Phone = phone });
            added++;
        }

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Импорт xlsx: добавлено {Added}, пропущено {Skipped}", added, skipped);

        return $"Добавлено {added}, пропущено {skipped}";
    }

    private async Task<List<ParcelRow>> RowsAsync(CancellationToken ct) =>
        await _db.Parcels
            .AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.PickupPoint)
            .OrderBy(p => p.Id)
            .Select(p => new ParcelRow
            {
                Track = p.TrackNumber,
                Client = p.Client!.FullName,
                Phone = p.Client.Phone,
                Point = p.PickupPoint!.Name,
                Status = p.Status.ToString(),
                Weight = p.Weight,
                Price = p.Price,
                ArrivedAt = p.ArrivedAt,
            })
            .ToListAsync(ct);

    private class ParcelRow
    {
        public string Track { get; set; } = string.Empty;

        public string Client { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Point { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Weight { get; set; }

        public decimal Price { get; set; }

        public DateTime ArrivedAt { get; set; }
    }

    private class ClientRow
    {
        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}
