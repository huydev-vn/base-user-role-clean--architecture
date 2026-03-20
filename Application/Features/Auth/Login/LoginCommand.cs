using Application.Common.Messaging;
using Application.DTOs.Auth;

namespace Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Username,
    string Password
) : ICommand<AuthResponse>;
