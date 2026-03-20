using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration cho bảng Users.
/// Senior devs dùng IEntityTypeConfiguration thay vì config trong OnModelCreating
/// để giữ mỗi bảng một file, dễ tìm, dễ maintain.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("Users");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL native UUID

        // ── Identity ──────────────────────────────────────────────────────
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        // Username phải unique — index tăng tốc lookup khi login
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Email vẫn unique để tránh duplicate account
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        // ── Profile ───────────────────────────────────────────────────────
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(512);

        // ── Auth ──────────────────────────────────────────────────────────
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()    // lưu "Admin", "User" thay vì số → dễ đọc trong DB
            .HasMaxLength(20);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(512);

        builder.Property(u => u.EmailVerificationToken)
            .HasMaxLength(256);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        // ── Audit ─────────────────────────────────────────────────────────
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(256);

        // ── Soft Delete ───────────────────────────────────────────────────
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Index cho soft delete — khi filter IsDeleted = false trên bảng lớn
        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        // ── Ignored (computed properties không map vào DB) ─────────────────
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.IsLocked);
        builder.Ignore(u => u.IsActive);
        builder.Ignore(u => u.DomainEvents);
    }
}
