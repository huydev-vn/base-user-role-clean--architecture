namespace Application.Common;

/// <summary>
/// Base cho tất cả Response DTO — mọi response đều có Id.
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
}
