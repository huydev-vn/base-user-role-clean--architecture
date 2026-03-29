using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Roles.AssignPermission;

internal sealed class AssignPermissionToRoleCommandHandler(
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignPermissionToRoleCommand>
{
    public async Task<Result> Handle(
        AssignPermissionToRoleCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission is null)
            throw new NotFoundException(nameof(Permission), command.PermissionId);

        if (!permission.IsActive)
            return Result.Failure("Permission đã bị vô hiệu hóa, không thể gán cho role.");

        var alreadyExists = await rolePermissionRepository.ExistsAsync(
            command.Role, command.PermissionId, cancellationToken);
        if (alreadyExists)
            return Result.Failure($"Role '{command.Role}' đã có permission '{permission.Name}'.");

        var rolePermission = RolePermission.Assign(command.Role, command.PermissionId);
        await rolePermissionRepository.AddAsync(rolePermission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
