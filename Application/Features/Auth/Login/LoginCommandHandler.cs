using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Auth.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IPermissionService permissionService,
    IMapper mapper)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        // Không tiết lộ user có tồn tại không — trả cùng message cho cả 2 trường hợp
        var user = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Failure("Username hoặc mật khẩu không đúng.");

        // Kiểm tra bị khóa tạm thời
        if (user.IsLocked)
            return Result<AuthResponse>.Failure(
                $"Tài khoản bị tạm khóa. Vui lòng thử lại sau {user.LockedUntil:HH:mm dd/MM/yyyy}.");

        // Kiểm tra trạng thái account
        if (user.Status == UserStatus.Banned)
            return Result<AuthResponse>.Failure("Tài khoản đã bị cấm vĩnh viễn.");

        if (user.Status == UserStatus.Inactive)
            return Result<AuthResponse>.Failure("Tài khoản đã bị vô hiệu hóa.");

        // Verify password — ghi nhận thất bại nếu sai
        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthResponse>.Failure("Username hoặc mật khẩu không đúng.");
        }

        // Đăng nhập thành công — reset counter + lấy permissions + cấp token mới
        user.RecordSuccessfulLogin();

        var permissions = await permissionService.GetEffectivePermissionNamesAsync(
            user.Id, user.Role, cancellationToken);

        var accessToken  = jwtTokenService.GenerateAccessToken(user, permissions);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiresAt: jwtTokenService.GetAccessTokenExpiry(),
            User: mapper.Map<UserDto>(user));

        return Result<AuthResponse>.Success(response);
    }
}
