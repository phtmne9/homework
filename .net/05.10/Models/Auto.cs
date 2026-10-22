namespace PagingApi.Models;

public class Auto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Producer { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public decimal Cost { get; set; }
}
