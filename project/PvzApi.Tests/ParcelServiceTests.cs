using Microsoft.Extensions.Logging.Abstractions;
using PvzApi.Entities;
using PvzApi.Models;
using PvzApi.Services;
using Xunit;

namespace PvzApi.Tests;

public class ParcelServiceTests
{
    private static ParcelService Service(TestDb db) =>
        new(db.Context, NullLogger<ParcelService>.Instance);

    [Fact]
    public async Task Accept_AddsParcel()
    {
        using var db = new TestDb();
        var service = Service(db);

        var result = await service.AcceptAsync(new ParcelCreateDto
        {
            TrackNumber = "PVZ-2",
            Weight = 2,
            Price = 1000,
            StorageDays = 5,
            PickupPointId = 1,
            ClientId = 1,
        });

        Assert.Contains("принята", result);
        Assert.Equal(2, db.Context.Parcels.Count());
    }

    [Fact]
    public async Task Accept_RejectsDuplicateTrack()
    {
        using var db = new TestDb();
        var service = Service(db);

        var result = await service.AcceptAsync(new ParcelCreateDto
        {
            TrackNumber = "PVZ-1",
            Weight = 1,
            Price = 1,
            StorageDays = 5,
            PickupPointId = 1,
            ClientId = 1,
        });

        Assert.Contains("уже принята", result);
    }

    [Fact]
    public async Task Accept_RejectsUnknownPoint()
    {
        using var db = new TestDb();
        var service = Service(db);

        var result = await service.AcceptAsync(new ParcelCreateDto
        {
            TrackNumber = "PVZ-3",
            Weight = 1,
            Price = 1,
            StorageDays = 5,
            PickupPointId = 99,
            ClientId = 1,
        });

        Assert.Contains("не найден", result);
    }

    [Fact]
    public async Task Issue_SucceedsForMatchingPhone()
    {
        using var db = new TestDb();
        var phone = db.Context.Clients.First(c => c.Id == 1).Phone;
        var service = Service(db);

        var result = await service.IssueAsync(1, phone);

        Assert.Contains("выдана", result);
        Assert.Equal(ParcelStatus.Issued, db.Context.Parcels.First(p => p.Id == 1).Status);
    }

    [Fact]
    public async Task Issue_RejectsWrongPhone()
    {
        using var db = new TestDb();
        var service = Service(db);

        var result = await service.IssueAsync(1, "+70000000000");

        Assert.Contains("не совпадает", result);
        Assert.Equal(ParcelStatus.InStock, db.Context.Parcels.First(p => p.Id == 1).Status);
    }

    [Fact]
    public async Task Issue_RejectsAlreadyIssued()
    {
        using var db = new TestDb();
        var phone = db.Context.Clients.First(c => c.Id == 1).Phone;
        var service = Service(db);

        await service.IssueAsync(1, phone);
        var second = await service.IssueAsync(1, phone);

        Assert.Contains("не на складе", second);
    }

    [Fact]
    public async Task Return_ChangesStatus()
    {
        using var db = new TestDb();
        var service = Service(db);

        var result = await service.ReturnAsync(1);

        Assert.Contains("возвращена", result);
        Assert.Equal(ParcelStatus.Returned, db.Context.Parcels.First(p => p.Id == 1).Status);
    }

    [Fact]
    public async Task GetPaged_SortsByPrice()
    {
        using var db = new TestDb();
        var service = Service(db);

        await service.AcceptAsync(new ParcelCreateDto
        {
            TrackNumber = "PVZ-9",
            Weight = 1,
            Price = 99999,
            StorageDays = 5,
            PickupPointId = 1,
            ClientId = 1,
        });

        var page = await service.GetPagedAsync(new ParcelQuery { SortBy = "price", Desc = true });

        Assert.Equal("PVZ-9", page.Items[0].TrackNumber);
    }

    [Fact]
    public async Task GetPaged_FiltersByStatus()
    {
        using var db = new TestDb();
        var service = Service(db);

        var inStock = await service.GetPagedAsync(new ParcelQuery { Status = ParcelStatus.InStock });
        var issued = await service.GetPagedAsync(new ParcelQuery { Status = ParcelStatus.Issued });

        Assert.Equal(1, inStock.Total);
        Assert.Equal(0, issued.Total);
    }
}
