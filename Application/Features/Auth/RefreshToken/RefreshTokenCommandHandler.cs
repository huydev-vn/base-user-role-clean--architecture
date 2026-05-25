using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Auth.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IPermissionService permissionService,
    IMapper mapper)
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        // Hash token nhận từ client trước khi tìm trong DB
        var hashedIncoming = jwtTokenService.HashRefreshToken(command.RefreshToken);

        var user = await userRepository.GetByRefreshTokenAsync(hashedIncoming, cancellationToken);

        if (user is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Result<AuthResponse>.Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");

        if (!user.IsActive)
            return Result<AuthResponse>.Forbidden("Tài khoản không còn hoạt động.");

        var permissions = await permissionService.GetEffectivePermissionNamesAsync(
            user.Id, user.Role, cancellationToken);

        // Rotate refresh token — tránh refresh token reuse attack
        var newAccessToken    = jwtTokenService.GenerateAccessToken(user, permissions);
        var rawNewRefresh     = jwtTokenService.GenerateRefreshToken();
        var hashedNewRefresh  = jwtTokenService.HashRefreshToken(rawNewRefresh);

        user.SetRefreshToken(hashedNewRefresh, DateTime.UtcNow.AddDays(7));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            AccessToken:          newAccessToken,
            RefreshToken:         rawNewRefresh,      // raw token trả về client
            AccessTokenExpiresAt: jwtTokenService.GetAccessTokenExpiry(),
            User:                 mapper.Map<UserDto>(user)));
    }
}
