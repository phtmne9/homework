using Asp.Versioning;
using AutoApi.Dtos;
using AutoApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AutoApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AutosController : ControllerBase
{
    private const string Read = "Авто — чтение";
    private const string Write = "Авто — изменение";

    private readonly IAutoService _autos;

    public AutosController(IAutoService autos) => _autos = autos;

    [HttpGet]
    [Tags(Read)]
    [SwaggerOperation(
        Summary = "Список автомобилей",
        Description = "Возвращает все записи таблицы Auto в порядке возрастания Id.")]
    [ProducesResponseType(typeof(IReadOnlyList<AutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AutoDto>>> GetAll(CancellationToken ct)
        => Ok(await _autos.GetAllAsync(ct));

    [HttpGet("{id:int}", Name = nameof(Get))]
    [Tags(Read)]
    [SwaggerOperation(
        Summary = "Автомобиль по Id",
        Description = "Возвращает одну запись. Если записи нет — 404.")]
    [ProducesResponseType(typeof(AutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutoDto>> Get(
        [SwaggerParameter("Идентификатор записи")] int id,
        CancellationToken ct)
    {
        var auto = await _autos.GetAsync(id, ct);

        if (auto is null)
            return NotFound();

        return Ok(auto);
    }

    [HttpPost]
    [Tags(Write)]
    [SwaggerOperation(
        Summary = "Создать автомобиль",
        Description = "Добавляет запись и возвращает её вместе с заголовком Location.")]
    [ProducesResponseType(typeof(AutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutoDto>> Create(AutoInput input, CancellationToken ct)
    {
        var auto = await _autos.CreateAsync(input, ct);

        return CreatedAtRoute(nameof(Get), new { id = auto.Id }, auto);
    }

    [HttpPut("{id:int}")]
    [Tags(Write)]
    [SwaggerOperation(
        Summary = "Заменить автомобиль",
        Description = "Полностью перезаписывает запись. Все поля обязательны.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, AutoInput input, CancellationToken ct)
    {
        if (!await _autos.UpdateAsync(id, input, ct))
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Tags(Write)]
    [SwaggerOperation(
        Summary = "Удалить автомобиль",
        Description = "Удаляет запись по Id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await _autos.DeleteAsync(id, ct))
            return NotFound();

        return NoContent();
    }
}
