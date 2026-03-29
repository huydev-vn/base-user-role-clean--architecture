using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.ChangeRole;

internal sealed class ChangeUserRoleCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPermissionService permissionService)
    : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<Result> Handle(
        ChangeUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        user.ChangeRole(command.NewRole);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache vì role mới sẽ có effective permissions khác
        await permissionService.InvalidateCacheAsync(command.UserId, cancellationToken);

        return Result.Success();
    }
}
