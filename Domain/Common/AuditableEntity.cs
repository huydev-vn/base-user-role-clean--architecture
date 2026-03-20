namespace Domain.Common;

/// <summary>
/// Kế thừa khi entity cần track: ai tạo, ai sửa, ai xóa, lúc nào.
/// Tất cả thao tác xóa là Soft Delete — không mất dữ liệu thật.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
