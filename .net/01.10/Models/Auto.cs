namespace AutoApi.Models;

public partial class Auto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Producer { get; set; } = null!;

    public string Color { get; set; } = null!;

    public decimal Cost { get; set; }
}
