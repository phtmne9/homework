using JwtApi.Data;
using JwtApi.Models;
using JwtApi.Security;
using JwtApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepo _users;
    private readonly ITokenService _tokens;

    public AuthController(IUserRepo users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> Token(LoginRequest req, CancellationToken ct)
    {
        var user = await _users.FindAsync(req.Login, ct);

        if (user is null || !Passwords.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        return Ok(_tokens.Create(user));
    }
}
