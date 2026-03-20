using Application.Common.Messaging;

namespace Application.Features.Users.Delete;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;
