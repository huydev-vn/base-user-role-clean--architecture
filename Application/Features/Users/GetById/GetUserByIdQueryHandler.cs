using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Users;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Users.GetById;

internal sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    IMapper mapper)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), query.UserId);

        return Result<UserDto>.Success(mapper.Map<UserDto>(user));
    }
}
