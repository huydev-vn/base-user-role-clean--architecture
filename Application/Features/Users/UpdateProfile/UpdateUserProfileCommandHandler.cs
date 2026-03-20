using Application.Common;
using Application.Common.Messaging;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Users.UpdateProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserProfileCommand>
{
    public async Task<Result> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        user.UpdateProfile(command.FirstName, command.LastName, command.PhoneNumber, command.AvatarUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
