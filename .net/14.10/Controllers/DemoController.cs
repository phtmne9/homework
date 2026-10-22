using System.Security.Claims;
using JwtApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DemoController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("open")]
    public IActionResult Open() => Ok(new { access = "anonymous" });

    [HttpGet("secure")]
    public IActionResult Secure() => Ok(new
    {
        login = User.Identity?.Name,
        role = User.FindFirstValue(ClaimTypes.Role),
        department = User.FindFirstValue(TokenService.DepartmentClaim),
    });

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin() => Ok(new { access = "Admin" });

    [Authorize(Policy = "ItOnly")]
    [HttpGet("it")]
    public IActionResult It() => Ok(new { access = "department=IT" });
}
