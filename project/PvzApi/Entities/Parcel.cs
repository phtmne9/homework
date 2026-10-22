namespace PvzApi.Entities;

public class Parcel
{
    public int Id { get; set; }

    public string TrackNumber { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal Price { get; set; }

    public ParcelStatus Status { get; set; }

    public DateTime ArrivedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public int StorageDays { get; set; }

    public int PickupPointId { get; set; }

    public PickupPoint? PickupPoint { get; set; }

    public int ClientId { get; set; }

    public Client? Client { get; set; }

    public DateTime Deadline => ArrivedAt.AddDays(StorageDays);
}
