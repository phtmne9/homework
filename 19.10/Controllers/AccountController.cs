using IdentityApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public AccountController(UserManager<IdentityUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (await _roles.RoleExistsAsync(request.Name))
            return Conflict($"Роль {request.Name} уже существует");

        var result = await _roles.CreateAsync(new IdentityRole(request.Name));

        return result.Succeeded
            ? Ok($"Роль {request.Name} создана")
            : BadRequest(result.Errors.Select(e => e.Description));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.UserName, Email = request.Email };
        var result = await _users.CreateAsync(user, request.Password);

        return result.Succeeded
            ? Ok($"Пользователь {request.UserName} создан")
            : BadRequest(result.Errors.Select(e => e.Description));
    }

    [HttpDelete("users/{userName}")]
    public async Task<IActionResult> DeleteUser(string userName)
    {
        var user = await _users.FindByNameAsync(userName);
        if (user is null)
            return NotFound($"Пользователь {userName} не найден");

        var result = await _users.DeleteAsync(user);

        return result.Succeeded
            ? Ok($"Пользователь {userName} удалён")
            : BadRequest(result.Errors.Select(e => e.Description));
    }

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _users.FindByNameAsync(request.UserName);
        if (user is null)
            return NotFound($"Пользователь {request.UserName} не найден");

        var result = await _users.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        return result.Succeeded
            ? Ok("Пароль изменён")
            : BadRequest(result.Errors.Select(e => e.Description));
    }
}
