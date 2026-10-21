using AutoFilesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoFilesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutosController : ControllerBase
{
    private const string Csv = "text/csv";

    private const string Xlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IAutoFileService _files;

    public AutosController(IAutoFileService files) => _files = files;

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv(CancellationToken ct)
        => File(await _files.ExportCsvAsync(ct), Csv, "autos.csv");

    [HttpGet("export/xlsx")]
    public async Task<IActionResult> ExportXlsx(CancellationToken ct)
        => File(await _files.ExportXlsxAsync(ct), Xlsx, "autos.xlsx");

    [HttpPost("import/csv")]
    public async Task<ActionResult<int>> ImportCsv(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();

        return await _files.ImportCsvAsync(stream, ct);
    }

    [HttpPost("import/xlsx")]
    public async Task<ActionResult<int>> ImportXlsx(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();

        return await _files.ImportXlsxAsync(stream, ct);
    }
}
