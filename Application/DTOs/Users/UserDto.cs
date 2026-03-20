using Domain.Enums;

namespace Application.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    UserRole Role,
    UserStatus Status,
    bool IsEmailVerified,
    DateTime CreatedAt
);
