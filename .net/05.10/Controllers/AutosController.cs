using Microsoft.AspNetCore.Mvc;
using PagingApi.Data;
using PagingApi.Models;

namespace PagingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutosController : ControllerBase
{
    private const int MaxSize = 200;

    private readonly IAutoRepo _repo;

    public AutosController(IAutoRepo repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<Paged<Auto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? producer = null,
        CancellationToken ct = default)
    {
        if (page < 1 || size is < 1 or > MaxSize)
            return BadRequest();

        return Ok(await _repo.GetPageAsync(page, size, producer, ct));
    }
}
