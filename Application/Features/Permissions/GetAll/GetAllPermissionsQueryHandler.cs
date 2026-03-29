using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Permissions.GetAll;

internal sealed class GetAllPermissionsQueryHandler(
    IPermissionRepository permissionRepository,
    IMapper mapper)
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        GetAllPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var permissions = query.ActiveOnly
            ? await permissionRepository.GetAllActiveAsync(cancellationToken)
            : await permissionRepository.GetAllAsync(cancellationToken);

        var dtos = permissions.Select(p => mapper.Map<PermissionDto>(p)).ToList();
        return Result<IReadOnlyList<PermissionDto>>.Success(dtos);
    }
}
