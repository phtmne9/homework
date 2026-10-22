using CityJoinsApi.Data;
using CityJoinsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityJoinsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JoinsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JoinsController(AppDbContext db) => _db = db;

    [HttpGet("join")]
    public IActionResult ByJoin()
    {
        var data = _db.Cities
            .AsNoTracking()
            .Join(_db.Countries,
                city => city.CountryId,
                country => country.Id,
                (city, country) => new CityView
                {
                    Id = city.Id,
                    City = city.Name,
                    Population = city.Population,
                    Country = country.Name,
                })
            .ToList();

        return Ok(data);
    }

    [HttpGet("include")]
    public IActionResult ByInclude()
    {
        var data = _db.Cities
            .AsNoTracking()
            .Include(c => c.Country)
            .Select(c => new CityView
            {
                Id = c.Id,
                City = c.Name,
                Population = c.Population,
                Country = c.Country!.Name,
            })
            .ToList();

        return Ok(data);
    }

    [HttpGet("load")]
    public IActionResult ByLoad()
    {
        var cities = _db.Cities.ToList();

        foreach (var city in cities)
            _db.Entry(city).Reference(c => c.Country).Load();

        return Ok(cities.Select(c => new CityView
        {
            Id = c.Id,
            City = c.Name,
            Population = c.Population,
            Country = c.Country?.Name ?? string.Empty,
        }));
    }

    [HttpGet("lazy")]
    public IActionResult ByLazy()
    {
        var cities = _db.Cities.ToList();

        return Ok(cities.Select(c => new CityView
        {
            Id = c.Id,
            City = c.Name,
            Population = c.Population,
            Country = c.Country?.Name ?? string.Empty,
        }));
    }
}
