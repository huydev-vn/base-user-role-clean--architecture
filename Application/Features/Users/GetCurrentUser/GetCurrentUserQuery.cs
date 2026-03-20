using Application.Common.Messaging;
using Application.DTOs.Users;

namespace Application.Features.Users.GetCurrentUser;

/// <summary>
/// Lấy thông tin user đang đăng nhập — dùng cho endpoint GET /api/users/me.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<UserDto>;
