using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Users.GetPermissions;

internal sealed class GetUserPermissionsQueryHandler(
    IUserRepository userRepository,
    IUserPermissionRepository userPermissionRepository,
    IMapper mapper)
    : IQueryHandler<GetUserPermissionsQuery, IReadOnlyList<UserPermissionDto>>
{
    public async Task<Result<IReadOnlyList<UserPermissionDto>>> Handle(
        GetUserPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var userExists = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (userExists is null)
            throw new NotFoundException(nameof(User), query.UserId);

        var userPermissions = await userPermissionRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        var dtos = userPermissions.Select(up => mapper.Map<UserPermissionDto>(up)).ToList();
        return Result<IReadOnlyList<UserPermissionDto>>.Success(dtos);
    }
}
