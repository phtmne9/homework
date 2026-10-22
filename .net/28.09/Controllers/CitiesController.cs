using CityEfApi.Models;
using CityEfApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityEfApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cities;

    public CitiesController(ICityService cities) => _cities = cities;

    [HttpGet]
    public IActionResult GetAll() => Ok(_cities.CityGetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var city = _cities.CityGetById(id);
        return city is null ? NotFound($"Город {id} не найден") : Ok(city);
    }

    [HttpPost]
    public IActionResult Add([FromBody] CityDTO city) => Ok(_cities.CityAdd(city));

    [HttpPut("{id:int}")]
    public IActionResult Edit(int id, [FromBody] CityDTO city) => Ok(_cities.CityEdit(city, id));

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) => Ok(_cities.CityDelete(id));
}
