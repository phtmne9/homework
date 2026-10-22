using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PvzApi.Data;
using PvzApi.Models;
using PvzApi.Security;
using PvzApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PvzApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Вход и получение токена")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthController(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("login")]
    [SwaggerOperation(Summary = "Получить JWT по логину и паролю")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == request.Login, ct);

        if (user is null || !Passwords.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Неверный логин или пароль");

        return Ok(_tokens.Create(user));
    }
}
