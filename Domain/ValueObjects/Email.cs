using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

/// <summary>
/// Email là Value Object — so sánh bằng giá trị, tự validate format khi tạo.
/// Không bao giờ tạo được Email với format sai.
/// </summary>
public sealed class Email : Common.ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Factory method — điểm duy nhất tạo Email, đảm bảo luôn hợp lệ.
    /// </summary>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new Email(value.ToLowerInvariant().Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
