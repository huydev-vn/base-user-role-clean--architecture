using Application.Common;
using Application.Common.Messaging;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Auth.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(
            u => u.EmailVerificationToken == command.Token,
            cancellationToken);

        if (user is null)
            return Result.Failure("Token xác thực không hợp lệ hoặc đã hết hạn.");

        // Idempotent — nếu đã verify rồi thì bỏ qua
        if (user.IsEmailVerified)
            return Result.Success();

        user.VerifyEmail(); // đổi Status → Active, clear token, raise UserActivatedEvent
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
