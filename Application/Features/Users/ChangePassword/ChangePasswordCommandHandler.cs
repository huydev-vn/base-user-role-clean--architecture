using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            return Result.Failure("Mật khẩu hiện tại không đúng.");

        user.ChangePassword(passwordHasher.Hash(command.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
