using Microsoft.EntityFrameworkCore;
using PvzApi.Data;
using PvzApi.Entities;
using PvzApi.Models;

namespace PvzApi.Services;

public class ParcelService : IParcelService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ParcelService> _log;

    public ParcelService(AppDbContext db, ILogger<ParcelService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<PageResult<ParcelDto>> GetPagedAsync(ParcelQuery q, CancellationToken ct = default)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var size = q.PageSize is < 1 or > 100 ? 20 : q.PageSize;

        var source = _db.Parcels
            .AsNoTracking()
            .Include(p => p.PickupPoint)
            .Include(p => p.Client)
            .AsQueryable();

        if (q.Status is not null)
            source = source.Where(p => p.Status == q.Status);

        if (q.PickupPointId is not null)
            source = source.Where(p => p.PickupPointId == q.PickupPointId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            source = source.Where(p =>
                p.TrackNumber.Contains(s) ||
                p.Client!.FullName.Contains(s) ||
                p.Client.Phone.Contains(s));
        }

        source = (q.SortBy?.ToLowerInvariant()) switch
        {
            "price" => q.Desc ? source.OrderByDescending(p => p.Price) : source.OrderBy(p => p.Price),
            "weight" => q.Desc ? source.OrderByDescending(p => p.Weight) : source.OrderBy(p => p.Weight),
            "track" => q.Desc ? source.OrderByDescending(p => p.TrackNumber) : source.OrderBy(p => p.TrackNumber),
            _ => q.Desc ? source.OrderByDescending(p => p.ArrivedAt) : source.OrderBy(p => p.ArrivedAt),
        };

        var total = await source.CountAsync(ct);

        var items = await source
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PageResult<ParcelDto>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = size,
            Total = total,
        };
    }

    public async Task<ParcelDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var parcel = await Query().FirstOrDefaultAsync(p => p.Id == id, ct);
        return parcel is null ? null : Map(parcel);
    }

    public async Task<ParcelDto?> GetByTrackAsync(string track, CancellationToken ct = default)
    {
        var parcel = await Query().FirstOrDefaultAsync(p => p.TrackNumber == track, ct);
        return parcel is null ? null : Map(parcel);
    }

    public async Task<string> AcceptAsync(ParcelCreateDto dto, CancellationToken ct = default)
    {
        if (await _db.Parcels.AnyAsync(p => p.TrackNumber == dto.TrackNumber, ct))
            return $"Посылка {dto.TrackNumber} уже принята";

        var point = await _db.PickupPoints.FirstOrDefaultAsync(p => p.Id == dto.PickupPointId, ct);
        if (point is null)
            return $"ПВЗ {dto.PickupPointId} не найден";

        if (!await _db.Clients.AnyAsync(c => c.Id == dto.ClientId, ct))
            return $"Клиент {dto.ClientId} не найден";

        var busy = await _db.Parcels
            .CountAsync(p => p.PickupPointId == point.Id && p.Status == ParcelStatus.InStock, ct);

        if (busy >= point.Capacity)
            return $"На ПВЗ {point.Name} нет свободных мест";

        _db.Parcels.Add(new Parcel
        {
            TrackNumber = dto.TrackNumber,
            Weight = dto.Weight,
            Price = dto.Price,
            StorageDays = dto.StorageDays,
            PickupPointId = dto.PickupPointId,
            ClientId = dto.ClientId,
            Status = ParcelStatus.InStock,
            ArrivedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Принята посылка {Track} на ПВЗ {Point}", dto.TrackNumber, point.Name);

        return $"Посылка {dto.TrackNumber} принята на {point.Name}";
    }

    public async Task<string> IssueAsync(int id, string phone, CancellationToken ct = default)
    {
        var parcel = await _db.Parcels
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (parcel is null)
            return $"Посылка {id} не найдена";

        if (parcel.Status != ParcelStatus.InStock)
            return $"Посылка {parcel.TrackNumber} не на складе, статус {parcel.Status}";

        if (parcel.Client!.Phone != phone)
            return "Телефон не совпадает с получателем";

        parcel.Status = ParcelStatus.Issued;
        parcel.IssuedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Выдана посылка {Track}", parcel.TrackNumber);

        return $"Посылка {parcel.TrackNumber} выдана";
    }

    public async Task<string> ReturnAsync(int id, CancellationToken ct = default)
    {
        var parcel = await _db.Parcels.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (parcel is null)
            return $"Посылка {id} не найдена";

        if (parcel.Status != ParcelStatus.InStock)
            return $"Вернуть можно только посылку на складе, статус {parcel.Status}";

        parcel.Status = ParcelStatus.Returned;
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Возвращена посылка {Track}", parcel.TrackNumber);

        return $"Посылка {parcel.TrackNumber} возвращена отправителю";
    }

    public async Task<IReadOnlyList<ParcelDto>> ExpiringAsync(int days, CancellationToken ct = default)
    {
        var limit = DateTime.UtcNow.AddDays(days);

        var parcels = await Query()
            .Where(p => p.Status == ParcelStatus.InStock)
            .ToListAsync(ct);

        return parcels
            .Where(p => p.Deadline <= limit)
            .OrderBy(p => p.Deadline)
            .Select(Map)
            .ToList();
    }

    private IQueryable<Parcel> Query() => _db.Parcels
        .AsNoTracking()
        .Include(p => p.PickupPoint)
        .Include(p => p.Client);

    private static ParcelDto Map(Parcel p) => new()
    {
        Id = p.Id,
        TrackNumber = p.TrackNumber,
        Weight = p.Weight,
        Price = p.Price,
        Status = p.Status,
        ArrivedAt = p.ArrivedAt,
        IssuedAt = p.IssuedAt,
        Deadline = p.ArrivedAt.AddDays(p.StorageDays),
        PickupPointName = p.PickupPoint == null ? string.Empty : p.PickupPoint.Name,
        ClientName = p.Client == null ? string.Empty : p.Client.FullName,
        ClientPhone = p.Client == null ? string.Empty : p.Client.Phone,
    };
}
