namespace CityJoinsApi.Models;

public class CityView
{
    public int Id { get; set; }

    public string City { get; set; } = string.Empty;

    public int Population { get; set; }

    public string Country { get; set; } = string.Empty;
}
