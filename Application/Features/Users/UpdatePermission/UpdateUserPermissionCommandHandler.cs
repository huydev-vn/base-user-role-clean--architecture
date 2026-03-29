using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.UpdatePermission;

internal sealed class UpdateUserPermissionCommandHandler(
    IUserPermissionRepository userPermissionRepository,
    IPermissionService permissionService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserPermissionCommand>
{
    public async Task<Result> Handle(
        UpdateUserPermissionCommand command,
        CancellationToken cancellationToken)
    {
        var userPermission = await userPermissionRepository.GetByUserAndPermissionAsync(
            command.UserId, command.PermissionId, cancellationToken);

        if (userPermission is null)
            throw new NotFoundException(nameof(UserPermission), $"{command.UserId}:{command.PermissionId}");

        userPermission.Update(command.IsGranted);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await permissionService.InvalidateCacheAsync(command.UserId, cancellationToken);

        return Result.Success();
    }
}
