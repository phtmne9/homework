using ContactApi.Entities;
using ContactApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private const string Xlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IContactService _contacts;

    public ContactsController(IContactService contacts) => _contacts = contacts;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Contact>>> GetAll()
        => Ok(await _contacts.GetAllAsync());

    [HttpPost("upload")]
    public async Task<ActionResult<int>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();

        return await _contacts.UploadAsync(stream, ct);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var data = await _contacts.ExportAsync();

        return File(data, Xlsx, "contacts.xlsx");
    }
}
