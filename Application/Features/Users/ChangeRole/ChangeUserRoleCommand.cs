using Application.Common.Messaging;
using Domain.Enums;

namespace Application.Features.Users.ChangeRole;

/// <summary>Admin only — đổi role của user bất kỳ.</summary>
public sealed record ChangeUserRoleCommand(Guid UserId, UserRole NewRole) : ICommand;
