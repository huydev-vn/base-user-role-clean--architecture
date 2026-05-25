using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Application.Settings;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MapsterMapper;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IPermissionService permissionService,
    IOptions<LockoutSettings> lockoutOptions,
    IMapper mapper)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Unauthorized("Username hoặc mật khẩu không đúng.");

        if (user.IsLocked)
            return Result<AuthResponse>.Unauthorized(
                $"Tài khoản bị tạm khóa. Vui lòng thử lại sau {user.LockedUntil:HH:mm dd/MM/yyyy}.");

        if (user.Status == UserStatus.Banned)
            return Result<AuthResponse>.Forbidden("Tài khoản đã bị cấm vĩnh viễn.");

        if (user.Status == UserStatus.Inactive)
            return Result<AuthResponse>.Forbidden("Tài khoản đã bị vô hiệu hóa.");

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            var settings = lockoutOptions.Value;
            user.RecordFailedLogin(settings.MaxFailedAttempts, settings.LockoutMinutes);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthResponse>.Unauthorized("Username hoặc mật khẩu không đúng.");
        }

        // Enforce email verification — PendingVerification không được login
        if (user.Status == UserStatus.PendingVerification)
            return Result<AuthResponse>.Forbidden(
                "Tài khoản chưa được xác thực. Vui lòng kiểm tra email và xác thực tài khoản.");

        user.RecordSuccessfulLogin();

        var permissions = await permissionService.GetEffectivePermissionNamesAsync(
            user.Id, user.Role, cancellationToken);

        var accessToken   = jwtTokenService.GenerateAccessToken(user, permissions);
        var rawRefresh    = jwtTokenService.GenerateRefreshToken();
        var hashedRefresh = jwtTokenService.HashRefreshToken(rawRefresh);

        user.SetRefreshToken(hashedRefresh, DateTime.UtcNow.AddDays(7));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            AccessToken:        accessToken,
            RefreshToken:       rawRefresh,        // raw token trả về client
            AccessTokenExpiresAt: jwtTokenService.GetAccessTokenExpiry(),
            User:               mapper.Map<UserDto>(user)));
    }
}
