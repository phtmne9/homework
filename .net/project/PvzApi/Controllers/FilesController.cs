using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PvzApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PvzApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Operator")]
[SwaggerTag("Выгрузка и загрузка файлов")]
public class FilesController : ControllerBase
{
    private readonly IFileService _files;

    public FilesController(IFileService files) => _files = files;

    [HttpGet("parcels.csv")]
    [SwaggerOperation(Summary = "Выгрузить посылки в csv")]
    public async Task<IActionResult> ExportCsv(CancellationToken ct) =>
        File(await _files.ExportParcelsCsvAsync(ct), "text/csv", "parcels.csv");

    [HttpGet("parcels.xlsx")]
    [SwaggerOperation(Summary = "Выгрузить посылки в xlsx")]
    public async Task<IActionResult> ExportXlsx(CancellationToken ct) =>
        File(
            await _files.ExportParcelsXlsxAsync(ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "parcels.xlsx");

    [HttpPost("clients/csv")]
    [SwaggerOperation(Summary = "Загрузить клиентов из csv")]
    public async Task<IActionResult> ImportCsv(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Файл не передан");

        await using var stream = file.OpenReadStream();
        return Ok(await _files.ImportClientsCsvAsync(stream, ct));
    }

    [HttpPost("clients/xlsx")]
    [SwaggerOperation(Summary = "Загрузить клиентов из xlsx")]
    public async Task<IActionResult> ImportXlsx(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Файл не передан");

        await using var stream = file.OpenReadStream();
        return Ok(await _files.ImportClientsXlsxAsync(stream, ct));
    }
}
