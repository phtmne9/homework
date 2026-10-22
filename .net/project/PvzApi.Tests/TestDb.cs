using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PvzApi.Data;
using PvzApi.Entities;

namespace PvzApi.Tests;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();

        Context.Parcels.Add(new Parcel
        {
            Id = 1,
            TrackNumber = "PVZ-1",
            Weight = 1.5m,
            Price = 5000,
            StorageDays = 7,
            ArrivedAt = DateTime.UtcNow.AddDays(-1),
            Status = ParcelStatus.InStock,
            ClientId = 1,
            PickupPointId = 1,
        });
        Context.SaveChanges();
    }

    public AppDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
