namespace Application.DTOs.Auth;

/// <summary>
/// Body của POST /api/auth/login.
/// </summary>
public sealed record LoginRequest(
    string Username,
    string Password
);
