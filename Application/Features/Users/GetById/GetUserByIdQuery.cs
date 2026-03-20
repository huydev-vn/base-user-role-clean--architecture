using Application.Common.Messaging;
using Application.DTOs.Users;

namespace Application.Features.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
