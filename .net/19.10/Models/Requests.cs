using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Models;

public class CreateRoleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
