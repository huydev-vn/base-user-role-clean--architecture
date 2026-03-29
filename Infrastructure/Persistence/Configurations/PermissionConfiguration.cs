using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration cho bảng Permissions.
/// Quy tắc:
///   - Name là unique key dùng làm policy name, lưu dạng "Users.Read".
///   - Group tự extract từ Name (phần trước '.' đầu tiên) — index tính năng tìm group.
///   - IsActive = true: permission có thể dùng. false: deactivated nhưng vẫn lưu audit.
///   - Dùng soft delete (thông qua AppDbContext global filter).
/// </summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("Permissions");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // ── Core Properties ───────────────────────────────────────────────
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Unique index — permission name là primary lookup key
        // Ví dụ: "Users.Read", "Reports.Export"
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Name");

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Group)
            .IsRequired()
            .HasMaxLength(50);

        // Index Group — tìm kiếm / filter theo nhóm chức năng trong admin UI
        builder.HasIndex(p => p.Group)
            .HasDatabaseName("IX_Permissions_Group");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index IsActive — filter permissions đang hoạt động
        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Permissions_IsActive");

        // ── Audit ─────────────────────────────────────────────────────────
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(256);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(256);

        // ── Soft Delete ───────────────────────────────────────────────────
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Permissions_IsDeleted");

        // ── Navigation ────────────────────────────────────────────────────
        // RolePermission: 1 Permission – * RolePermissions
        builder
            .HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserPermission: 1 Permission – * UserPermissions
        builder
            .HasMany(p => p.UserPermissions)
            .WithOne(up => up.Permission)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Ignored ───────────────────────────────────────────────────────
        builder.Ignore(p => p.DomainEvents);
    }
}
