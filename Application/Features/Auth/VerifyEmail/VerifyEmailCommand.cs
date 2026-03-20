using Application.Common.Messaging;

namespace Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : ICommand;
