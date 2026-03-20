using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.ChangeRole;

internal sealed class ChangeUserRoleCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
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

        return Result.Success();
    }
}
