using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Users;
using Domain.Enums;

namespace Application.Features.Users.GetAll;

/// <summary>
/// Query lấy danh sách users có phân trang, tìm kiếm và lọc.
/// Admin only — controller phải kiểm tra quyền trước khi gửi query.
/// </summary>
public sealed record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    UserRole? Role = null,
    UserStatus? Status = null
) : IQuery<PagedResult<UserDto>>;
