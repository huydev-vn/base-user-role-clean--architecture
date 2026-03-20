using Application.Common.Messaging;
using Application.DTOs.Auth;

namespace Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
) : ICommand<AuthResponse>;
