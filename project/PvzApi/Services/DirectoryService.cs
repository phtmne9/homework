using Microsoft.EntityFrameworkCore;
using PvzApi.Data;
using PvzApi.Entities;
using PvzApi.Models;

namespace PvzApi.Services;

public class DirectoryService : IDirectoryService
{
    private readonly AppDbContext _db;

    public DirectoryService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PickupPointDto>> GetPointsAsync(CancellationToken ct = default) =>
        await _db.PickupPoints
            .AsNoTracking()
            .Select(p => new PickupPointDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Capacity = p.Capacity,
                InStock = p.Parcels.Count(x => x.Status == ParcelStatus.InStock),
            })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ClientDto>> GetClientsAsync(CancellationToken ct = default) =>
        await _db.Clients
            .AsNoTracking()
            .Select(c => new ClientDto { Id = c.Id, FullName = c.FullName, Phone = c.Phone })
            .ToListAsync(ct);

    public async Task<string> AddClientAsync(ClientDto dto, CancellationToken ct = default)
    {
        if (await _db.Clients.AnyAsync(c => c.Phone == dto.Phone, ct))
            return $"Клиент с телефоном {dto.Phone} уже есть";

        _db.Clients.Add(new Client { FullName = dto.FullName, Phone = dto.Phone });
        await _db.SaveChangesAsync(ct);

        return $"Клиент {dto.FullName} добавлен";
    }

    public async Task<string> AddPointAsync(PickupPointDto dto, CancellationToken ct = default)
    {
        _db.PickupPoints.Add(new PickupPoint
        {
            Name = dto.Name,
            Address = dto.Address,
            Capacity = dto.Capacity,
        });
        await _db.SaveChangesAsync(ct);

        return $"ПВЗ {dto.Name} добавлен";
    }
}
