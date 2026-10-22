using System.Text;
using CityApi.Data;
using CityApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityApi.Services;

public class CityFileService : ICityFileService
{
    private readonly AppDbContext _db;

    public CityFileService(AppDbContext db) => _db = db;

    public async Task<int> UploadAsync(Stream file, CancellationToken ct)
    {
        var cities = new List<City>();

        using var reader = new StreamReader(file);

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            var p = line.Split(';', StringSplitOptions.TrimEntries);

            if (p.Length != 3 || p[0].Length == 0 || !int.TryParse(p[2], out var pop))
                continue;

            cities.Add(new City { Name = p[0], Country = p[1], Population = pop });
        }

        if (cities.Count == 0)
            return 0;

        _db.Cities.AddRange(cities);
        await _db.SaveChangesAsync(ct);

        return cities.Count;
    }

    public async Task<byte[]> DownloadAsync(CancellationToken ct)
    {
        var cities = await _db.Cities
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new { c.Name, c.Country, c.Population })
            .ToListAsync(ct);

        var text = string.Join(
            Environment.NewLine,
            cities.Select(c => $"{c.Name};{c.Country};{c.Population}"));

        return Encoding.UTF8.GetBytes(text);
    }
}
