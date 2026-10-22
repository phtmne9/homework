using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PvzApi.Features.Parcels;
using PvzApi.Models;
using PvzApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PvzApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
[SwaggerTag("Посылки: приёмка, выдача, возврат")]
public class ParcelsController : ControllerBase
{
    private readonly IParcelService _parcels;
    private readonly IMediator _mediator;
    private readonly IValidator<ParcelCreateDto> _validator;

    public ParcelsController(
        IParcelService parcels, IMediator mediator, IValidator<ParcelCreateDto> validator)
    {
        _parcels = parcels;
        _mediator = mediator;
        _validator = validator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список посылок с пагинацией, фильтром и сортировкой")]
    public async Task<IActionResult> Get([FromQuery] ParcelQuery query, CancellationToken ct) =>
        Ok(await _parcels.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Посылка по идентификатору")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var parcel = await _parcels.GetByIdAsync(id, ct);
        return parcel is null ? NotFound($"Посылка {id} не найдена") : Ok(parcel);
    }

    [HttpGet("track/{trackNumber}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Отслеживание по трек-номеру, без авторизации")]
    public async Task<IActionResult> GetByTrack(string trackNumber, CancellationToken ct)
    {
        var parcel = await _parcels.GetByTrackAsync(trackNumber, ct);
        return parcel is null ? NotFound($"Посылка {trackNumber} не найдена") : Ok(parcel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    [SwaggerOperation(Summary = "Принять посылку на ПВЗ")]
    public async Task<IActionResult> Accept([FromBody] ParcelCreateDto parcel, CancellationToken ct)
    {
        var check = await _validator.ValidateAsync(parcel, ct);

        if (!check.IsValid)
            return BadRequest(check.Errors.Select(e => e.ErrorMessage));

        return Ok(await _parcels.AcceptAsync(parcel, ct));
    }

    [HttpPost("{id:int}/issue")]
    [Authorize(Roles = "Admin,Operator")]
    [SwaggerOperation(Summary = "Выдать посылку клиенту по телефону")]
    public async Task<IActionResult> Issue(int id, [FromBody] IssueRequest request) =>
        Ok(await _mediator.Send(new IssueParcelCommand(id, request.Phone)));

    [HttpPost("{id:int}/return")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Вернуть посылку отправителю")]
    public async Task<IActionResult> Return(int id) =>
        Ok(await _mediator.Send(new ReturnParcelCommand(id)));

    [HttpGet("expiring")]
    [SwaggerOperation(Summary = "Посылки с истекающим сроком хранения")]
    public async Task<IActionResult> Expiring([FromQuery] int days, CancellationToken ct) =>
        Ok(await _parcels.ExpiringAsync(days <= 0 ? 2 : days, ct));
}
