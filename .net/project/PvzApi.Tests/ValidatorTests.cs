using PvzApi.Models;
using PvzApi.Validators;
using Xunit;

namespace PvzApi.Tests;

public class ValidatorTests
{
    private readonly ParcelCreateValidator _parcel = new();
    private readonly ClientValidator _client = new();

    [Theory]
    [InlineData("", 1, 1, 5)]
    [InlineData("PVZ 1", 1, 1, 5)]
    [InlineData("PVZ-1", 0, 1, 5)]
    [InlineData("PVZ-1", 1, -1, 5)]
    [InlineData("PVZ-1", 1, 1, 0)]
    public void Parcel_Invalid(string track, decimal weight, decimal price, int days)
    {
        var result = _parcel.Validate(new ParcelCreateDto
        {
            TrackNumber = track,
            Weight = weight,
            Price = price,
            StorageDays = days,
            PickupPointId = 1,
            ClientId = 1,
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Parcel_Valid()
    {
        var result = _parcel.Validate(new ParcelCreateDto
        {
            TrackNumber = "PVZ-100500",
            Weight = 2.5m,
            Price = 15000,
            StorageDays = 7,
            PickupPointId = 1,
            ClientId = 1,
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "+77011112233")]
    [InlineData("Имя", "87011112233")]
    [InlineData("Имя", "+7701")]
    public void Client_Invalid(string name, string phone)
    {
        Assert.False(_client.Validate(new ClientDto { FullName = name, Phone = phone }).IsValid);
    }

    [Fact]
    public void Client_Valid()
    {
        Assert.True(_client.Validate(
            new ClientDto { FullName = "Плисак Арсений", Phone = "+77011112233" }).IsValid);
    }
}
