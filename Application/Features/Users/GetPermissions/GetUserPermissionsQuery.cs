using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;

namespace Application.Features.Users.GetPermissions;

/// <summary>Lấy danh sách permission overrides của một user — dành cho admin UI.</summary>
public sealed record GetUserPermissionsQuery(Guid UserId) : IQuery<IReadOnlyList<UserPermissionDto>>;
