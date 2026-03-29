using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Roles.RevokePermission;

internal sealed class RevokePermissionFromRoleCommandHandler(
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokePermissionFromRoleCommand>
{
    public async Task<Result> Handle(
        RevokePermissionFromRoleCommand command,
        CancellationToken cancellationToken)
    {
        var rolePermission = await rolePermissionRepository.GetByRoleAndPermissionAsync(
            command.Role, command.PermissionId, cancellationToken);

        if (rolePermission is null)
            throw new NotFoundException(nameof(RolePermission), $"{command.Role}:{command.PermissionId}");

        rolePermission.Revoke();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
