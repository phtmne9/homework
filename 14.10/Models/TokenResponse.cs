namespace JwtApi.Models;

public record TokenResponse(string Token, DateTime ExpiresAt, string Role, string Department);
