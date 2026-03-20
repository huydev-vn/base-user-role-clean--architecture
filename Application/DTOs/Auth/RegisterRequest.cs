namespace Application.DTOs.Auth;

/// <summary>
/// Body của POST /api/auth/register.
/// Controller nhận DTO này và map sang RegisterCommand.
/// </summary>
public sealed record RegisterRequest(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
);
