using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;
using Domain.Enums;

namespace Application.Features.Roles.GetPermissions;

/// <summary>Lấy danh sách permissions được gán cho một role — dành cho admin UI.</summary>
public sealed record GetRolePermissionsQuery(UserRole Role) : IQuery<IReadOnlyList<RolePermissionDto>>;
