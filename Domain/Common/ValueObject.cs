namespace Domain.Common;

/// <summary>
/// Base cho Value Objects — các object được so sánh bằng GIÁ TRỊ, không phải reference.
/// Ví dụ: Email("a@b.com") == Email("a@b.com") → true, dù là 2 object khác nhau.
/// Dùng cho: Email, PhoneNumber, Money, Address, Coordinate...
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Subclass khai báo những giá trị nào tham gia so sánh bằng.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
