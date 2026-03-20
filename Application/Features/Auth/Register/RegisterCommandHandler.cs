using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Auth.Register;

internal sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IMapper mapper)
    : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra username / email chưa tồn tại
        if (await userRepository.IsUsernameTakenAsync(command.Username, cancellationToken))
            return Result<AuthResponse>.Failure("Username đã được sử dụng.");

        if (await userRepository.IsEmailTakenAsync(command.Email, cancellationToken))
            return Result<AuthResponse>.Failure("Email đã được sử dụng.");

        // 2. Hash password
        var passwordHash = passwordHasher.Hash(command.Password);

        // 3. Tạo email verification token (thực tế sẽ gửi qua email service)
        var verificationToken = Guid.NewGuid().ToString("N");

        // 4. Tạo User entity — business logic nằm trong factory method
        var user = User.Create(
            command.Username,
            command.FirstName,
            command.LastName,
            command.Email,
            passwordHash,
            verificationToken);

        // 5. Generate tokens và gắn refresh token vào user
        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        // 6. Lưu vào DB (UnitOfWork sẽ dispatch domain events sau khi save)
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiresAt: jwtTokenService.GetAccessTokenExpiry(),
            User: mapper.Map<UserDto>(user));

        return Result<AuthResponse>.Success(response);
    }
}
