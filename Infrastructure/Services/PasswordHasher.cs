using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// BCrypt implementation — work factor 12 đủ an toàn cho production (2026).
/// EnhancedHashPassword dùng SHA-384 trước khi hash → an toàn hơn với password dài.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}
