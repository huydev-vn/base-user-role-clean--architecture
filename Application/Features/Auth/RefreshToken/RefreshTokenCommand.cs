using Application.Common.Messaging;
using Application.DTOs.Auth;

namespace Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;
