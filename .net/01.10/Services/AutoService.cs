using AutoApi.Dtos;
using AutoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoApi.Services;

public class AutoService : IAutoService
{
    private readonly AutoDbContext _db;

    public AutoService(AutoDbContext db) => _db = db;

    public async Task<IReadOnlyList<AutoDto>> GetAllAsync(CancellationToken ct)
        => await _db.Autos
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new AutoDto(a.Id, a.Name, a.Producer, a.Color, a.Cost))
            .ToListAsync(ct);

    public async Task<AutoDto?> GetAsync(int id, CancellationToken ct)
        => await _db.Autos
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AutoDto(a.Id, a.Name, a.Producer, a.Color, a.Cost))
            .FirstOrDefaultAsync(ct);

    public async Task<AutoDto> CreateAsync(AutoInput input, CancellationToken ct)
    {
        var auto = new Auto
        {
            Name = input.Name,
            Producer = input.Producer,
            Color = input.Color,
            Cost = input.Cost,
        };

        _db.Autos.Add(auto);
        await _db.SaveChangesAsync(ct);

        return new AutoDto(auto.Id, auto.Name, auto.Producer, auto.Color, auto.Cost);
    }

    public async Task<bool> UpdateAsync(int id, AutoInput input, CancellationToken ct)
    {
        var auto = await _db.Autos.FirstOrDefaultAsync(a => a.Id == id, ct);

        if (auto is null)
            return false;

        auto.Name = input.Name;
        auto.Producer = input.Producer;
        auto.Color = input.Color;
        auto.Cost = input.Cost;

        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var auto = await _db.Autos.FirstOrDefaultAsync(a => a.Id == id, ct);

        if (auto is null)
            return false;

        _db.Autos.Remove(auto);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}
