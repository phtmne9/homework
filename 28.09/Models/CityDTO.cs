namespace CityEfApi.Models;

public class CityDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Population { get; set; }

    public int CountryId { get; set; }

    public string CountryName { get; set; } = string.Empty;
}
