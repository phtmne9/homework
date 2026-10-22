using CityApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityFileService _cities;

    public CitiesController(ICityFileService cities) => _cities = cities;

    [HttpPost("upload")]
    public async Task<ActionResult<int>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();

        return await _cities.UploadAsync(stream, ct);
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download(CancellationToken ct)
    {
        var data = await _cities.DownloadAsync(ct);

        return File(data, "text/plain", "cities.txt");
    }
}
