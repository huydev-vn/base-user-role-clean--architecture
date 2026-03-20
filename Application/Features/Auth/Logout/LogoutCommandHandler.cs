using Application.Common;
using Application.Common.Messaging;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Auth.Logout;

internal sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
            return Result.Failure("Chưa đăng nhập.");

        var userId = Guid.Parse(currentUserService.UserId);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        // Idempotent — không throw nếu user không tồn tại
        if (user is null) return Result.Success();

        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
