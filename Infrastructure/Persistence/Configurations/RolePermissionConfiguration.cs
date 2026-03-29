using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration cho bảng RolePermissions.
/// Quy tắc:
///   - Composite key (Role, PermissionId) đảm bảo không duplicate mapping.
///   - FK tới Permissions dùng Cascade delete — xóa permission sẽ xóa mapping này.
///   - IsDeleted = true (soft delete) khi permission bị revoke khỏi role.
///   - Application layer kiểm tra EXISTS trước khi Assign() để tránh duplicate.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("RolePermissions");

        // ── Primary Key ───────────────────────────────────────────────────
        // Composite key: (Role, PermissionId)
        builder.HasKey(rp => new { rp.Role, rp.PermissionId })
            .HasName("PK_RolePermissions_RolePermissionId");

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(rp => rp.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        // ── Foreign Keys ──────────────────────────────────────────────────
        builder
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .HasConstraintName("FK_RolePermissions_PermissionId")
            .OnDelete(DeleteBehavior.Cascade); // Xóa permission → xóa mapping

        // ── Indexes ───────────────────────────────────────────────────────
        // Tìm tất cả permissions của một role
        builder.HasIndex(rp => rp.Role)
            .HasDatabaseName("IX_RolePermissions_Role");

        // Tìm tất cả roles có một permission cụ thể
        builder.HasIndex(rp => rp.PermissionId)
            .HasDatabaseName("IX_RolePermissions_PermissionId");

        // ── Audit ─────────────────────────────────────────────────────────
        builder.Property(rp => rp.CreatedAt)
            .IsRequired();

        builder.Property(rp => rp.CreatedBy)
            .HasMaxLength(256);

        builder.Property(rp => rp.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(rp => rp.DeletedBy)
            .HasMaxLength(256);

        // ── Soft Delete ───────────────────────────────────────────────────
        builder.Property(rp => rp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(rp => rp.IsDeleted)
            .HasDatabaseName("IX_RolePermissions_IsDeleted");

        // ── Ignored ───────────────────────────────────────────────────────
        builder.Ignore(rp => rp.Id);
        builder.Ignore(rp => rp.DomainEvents);
    }
}
