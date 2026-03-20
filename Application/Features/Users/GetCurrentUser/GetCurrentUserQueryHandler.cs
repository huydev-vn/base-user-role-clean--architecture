using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Users;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Users.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IMapper mapper)
    : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
            return Result<UserDto>.Failure("Chưa đăng nhập.");

        var userId = Guid.Parse(currentUserService.UserId);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        return Result<UserDto>.Success(mapper.Map<UserDto>(user));
    }
}
