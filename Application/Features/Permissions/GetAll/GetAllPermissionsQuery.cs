using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Permissions;

namespace Application.Features.Permissions.GetAll;

/// <summary>Lấy toàn bộ danh sách permissions — dành cho admin UI.</summary>
public sealed record GetAllPermissionsQuery(bool ActiveOnly = false) : IQuery<IReadOnlyList<PermissionDto>>;
