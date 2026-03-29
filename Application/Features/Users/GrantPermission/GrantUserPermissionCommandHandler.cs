using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.GrantPermission;

internal sealed class GrantUserPermissionCommandHandler(
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IUserPermissionRepository userPermissionRepository,
    IPermissionService permissionService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GrantUserPermissionCommand>
{
    public async Task<Result> Handle(
        GrantUserPermissionCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        var permission = await permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission is null)
            throw new NotFoundException(nameof(Permission), command.PermissionId);

        if (!permission.IsActive)
            return Result.Failure("Permission đã bị vô hiệu hóa, không thể cấp cho user.");

        var alreadyExists = await userPermissionRepository.ExistsAsync(
            command.UserId, command.PermissionId, cancellationToken);
        if (alreadyExists)
            return Result.Failure($"User đã có override cho permission '{permission.Name}'. Dùng Update để thay đổi.");

        var userPermission = UserPermission.Grant(command.UserId, command.PermissionId);
        await userPermissionRepository.AddAsync(userPermission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await permissionService.InvalidateCacheAsync(command.UserId, cancellationToken);

        return Result.Success();
    }
}
