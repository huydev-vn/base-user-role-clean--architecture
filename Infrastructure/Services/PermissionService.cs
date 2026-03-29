using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

/// <summary>
/// Tính toán và cache effective permissions cho user.
///
/// Chiến lược cache: in-memory với TTL 5 phút.
///   - Đủ ngắn để thay đổi permissions phản ánh nhanh (5 phút).
///   - Đủ dài để tránh round-trip DB trên mọi request.
/// Khi Admin thay đổi UserPermission, gọi InvalidateCacheAsync(userId) để force refresh ngay.
/// Khi Admin thay đổi RolePermission, cache tự expire sau TTL (role changes hiếm).
/// </summary>
internal sealed class PermissionService(
    IUserPermissionRepository userPermissionRepository,
    IMemoryCache cache)
    : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyList<string>> GetEffectivePermissionNamesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(userId);

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            return cached;

        var permissions = await userPermissionRepository
            .GetEffectivePermissionNamesAsync(userId, role, cancellationToken);

        cache.Set(cacheKey, permissions, CacheTtl);

        return permissions;
    }

    public Task InvalidateCacheAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cache.Remove(BuildCacheKey(userId));
        return Task.CompletedTask;
    }

    private static string BuildCacheKey(Guid userId) => $"permissions:{userId}";
}
