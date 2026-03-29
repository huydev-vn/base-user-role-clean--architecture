using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration cho bảng UserPermissions.
/// Quy tắc:
///   - Composite key (UserId, PermissionId) tương tự RolePermission.
///   - IsGranted = true: cấp thêm permission vượt ngoài role.
///   - IsGranted = false: thu hồi permission dù role có quyền.
///   - IsDeleted = true (soft delete) khi override bị hủy.
///   - Application layer kiểm tra EXISTS trước khi Grant/Deny.
/// </summary>
public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("UserPermissions");

        // ── Primary Key ───────────────────────────────────────────────────
        // Composite key: (UserId, PermissionId)
        builder.HasKey(up => new { up.UserId, up.PermissionId })
            .HasName("PK_UserPermissions_UserIdPermissionId");

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.PermissionId)
            .IsRequired();

        builder.Property(up => up.IsGranted)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Foreign Keys ──────────────────────────────────────────────────
        // FK tới User: 1 User – * UserPermissions
        builder
            .HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId)
            .HasConstraintName("FK_UserPermissions_UserId")
            .OnDelete(DeleteBehavior.Cascade); // Xóa user → xóa overrides

        // FK tới Permission: 1 Permission – * UserPermissions
        builder
            .HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId)
            .HasConstraintName("FK_UserPermissions_PermissionId")
            .OnDelete(DeleteBehavior.Cascade); // Xóa permission → xóa overrides

        // ── Indexes ───────────────────────────────────────────────────────
        // Tìm tất cả permission overrides của một user
        builder.HasIndex(up => up.UserId)
            .HasDatabaseName("IX_UserPermissions_UserId");

        // Tìm tất cả user-level overrides cho một permission
        builder.HasIndex(up => up.PermissionId)
            .HasDatabaseName("IX_UserPermissions_PermissionId");

        // Index IsGranted — filter grants vs denies
        builder.HasIndex(up => up.IsGranted)
            .HasDatabaseName("IX_UserPermissions_IsGranted");

        // Composite index cho hiệu suất: (UserId, IsGranted)
        builder.HasIndex(up => new { up.UserId, up.IsGranted })
            .HasDatabaseName("IX_UserPermissions_UserId_IsGranted");

        // ── Audit ─────────────────────────────────────────────────────────
        builder.Property(up => up.CreatedAt)
            .IsRequired();

        builder.Property(up => up.CreatedBy)
            .HasMaxLength(256);

        builder.Property(up => up.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(up => up.DeletedBy)
            .HasMaxLength(256);

        // ── Soft Delete ───────────────────────────────────────────────────
        builder.Property(up => up.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(up => up.IsDeleted)
            .HasDatabaseName("IX_UserPermissions_IsDeleted");

        // ── Ignored ───────────────────────────────────────────────────────
        builder.Ignore(up => up.Id);
        builder.Ignore(up => up.DomainEvents);
    }
}
