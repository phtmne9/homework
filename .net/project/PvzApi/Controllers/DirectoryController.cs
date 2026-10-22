using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PvzApi.Models;
using PvzApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PvzApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Справочники: ПВЗ и клиенты")]
public class DirectoryController : ControllerBase
{
    private readonly IDirectoryService _directory;
    private readonly IValidator<ClientDto> _validator;

    public DirectoryController(IDirectoryService directory, IValidator<ClientDto> validator)
    {
        _directory = directory;
        _validator = validator;
    }

    [HttpGet("points")]
    [SwaggerOperation(Summary = "Список ПВЗ с текущей загрузкой")]
    public async Task<IActionResult> Points(CancellationToken ct) =>
        Ok(await _directory.GetPointsAsync(ct));

    [HttpGet("clients")]
    [SwaggerOperation(Summary = "Список клиентов")]
    public async Task<IActionResult> Clients(CancellationToken ct) =>
        Ok(await _directory.GetClientsAsync(ct));

    [HttpPost("clients")]
    [Authorize(Roles = "Admin,Operator")]
    [SwaggerOperation(Summary = "Добавить клиента")]
    public async Task<IActionResult> AddClient([FromBody] ClientDto client, CancellationToken ct)
    {
        var check = await _validator.ValidateAsync(client, ct);

        if (!check.IsValid)
            return BadRequest(check.Errors.Select(e => e.ErrorMessage));

        return Ok(await _directory.AddClientAsync(client, ct));
    }

    [HttpPost("points")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Добавить ПВЗ")]
    public async Task<IActionResult> AddPoint([FromBody] PickupPointDto point, CancellationToken ct) =>
        Ok(await _directory.AddPointAsync(point, ct));
}
