using MathApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MathApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MathController : ControllerBase
{
    private const int MaxCount = 1000;

    private readonly IMathService _math;

    public MathController(IMathService math) => _math = math;

    [HttpGet("circle-area")]
    public ActionResult<double> CircleArea([FromQuery] double r)
    {
        if (r <= 0)
            return BadRequest();

        return _math.CircleArea(r);
    }

    [HttpGet("sphere-volume")]
    public ActionResult<double> SphereVolume([FromQuery] double r)
    {
        if (r <= 0)
            return BadRequest();

        return _math.SphereVolume(r);
    }

    [HttpGet("palindromes")]
    public ActionResult<IReadOnlyList<int>> Palindromes(
        [FromQuery] int count = 10,
        [FromQuery] int from = 1)
    {
        if (count is < 1 or > MaxCount || from < 0)
            return BadRequest();

        return Ok(_math.Palindromes(count, from));
    }
}
