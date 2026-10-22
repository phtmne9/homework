namespace CityJoinsApi.Entities;

public class City
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Population { get; set; }

    public int CountryId { get; set; }

    public virtual Country? Country { get; set; }
}
