using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.Features.Auth.Register;

internal sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        if (await userRepository.IsUsernameTakenAsync(command.Username, cancellationToken))
            return Result<RegisterResponse>.Conflict("Username đã được sử dụng.");

        if (await userRepository.IsEmailTakenAsync(command.Email, cancellationToken))
            return Result<RegisterResponse>.Conflict("Email đã được sử dụng.");

        var passwordHash = passwordHasher.Hash(command.Password);

        // Thực tế: gửi token này qua email service
        var verificationToken = Guid.NewGuid().ToString("N");

        var user = User.Create(
            command.Username,
            command.FirstName,
            command.LastName,
            command.Email,
            passwordHash,
            verificationToken);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            UserId:  user.Id,
            Username: user.Username,
            Email:   user.Email,
            Message: "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản trước khi đăng nhập."));
    }
}
