namespace PvzApi.Entities;

public class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public List<Parcel> Parcels { get; set; } = new();
}
