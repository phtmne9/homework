using CityEfApi.Data;
using CityEfApi.Entities;
using CityEfApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CityEfApi.Services;

public class CityService : ICityService
{
    private readonly AppDbContext _db;

    public CityService(AppDbContext db) => _db = db;

    public IEnumerable<CityDTO> CityGetAll() =>
        _db.Cities
            .AsNoTracking()
            .Include(c => c.Country)
            .Select(c => Map(c))
            .ToList();

    public CityDTO? CityGetById(int id)
    {
        var city = _db.Cities
            .AsNoTracking()
            .Include(c => c.Country)
            .FirstOrDefault(c => c.Id == id);

        return city is null ? null : Map(city);
    }

    public string CityAdd(CityDTO city)
    {
        if (string.IsNullOrWhiteSpace(city.Name))
            return "Название города обязательно";

        if (!_db.Countries.Any(c => c.Id == city.CountryId))
            return $"Страна {city.CountryId} не найдена";

        _db.Cities.Add(new City
        {
            Name = city.Name,
            Population = city.Population,
            CountryId = city.CountryId,
        });
        _db.SaveChanges();

        return "Город добавлен";
    }

    public string CityEdit(CityDTO city, int id)
    {
        var found = _db.Cities.FirstOrDefault(c => c.Id == id);
        if (found is null)
            return $"Город {id} не найден";

        if (string.IsNullOrWhiteSpace(city.Name))
            return "Название города обязательно";

        if (!_db.Countries.Any(c => c.Id == city.CountryId))
            return $"Страна {city.CountryId} не найдена";

        found.Name = city.Name;
        found.Population = city.Population;
        found.CountryId = city.CountryId;
        _db.SaveChanges();

        return "Город изменён";
    }

    public string CityDelete(int id)
    {
        var found = _db.Cities.FirstOrDefault(c => c.Id == id);
        if (found is null)
            return $"Город {id} не найден";

        _db.Cities.Remove(found);
        _db.SaveChanges();

        return "Город удалён";
    }

    private static CityDTO Map(City c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Population = c.Population,
        CountryId = c.CountryId,
        CountryName = c.Country?.Name ?? string.Empty,
    };
}
