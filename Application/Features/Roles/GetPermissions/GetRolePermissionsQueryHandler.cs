using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Roles.GetPermissions;

internal sealed class GetRolePermissionsQueryHandler(
    IRolePermissionRepository rolePermissionRepository,
    IMapper mapper)
    : IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>>
{
    public async Task<Result<IReadOnlyList<RolePermissionDto>>> Handle(
        GetRolePermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var rolePermissions = await rolePermissionRepository.GetByRoleAsync(query.Role, cancellationToken);
        var dtos = rolePermissions.Select(rp => mapper.Map<RolePermissionDto>(rp)).ToList();
        return Result<IReadOnlyList<RolePermissionDto>>.Success(dtos);
    }
}
