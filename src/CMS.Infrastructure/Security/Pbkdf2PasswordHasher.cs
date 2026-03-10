using System.Security.Cryptography;
using CMS.Application.Interfaces.Services;

namespace CMS.Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 120_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string password, string expectedHash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);

        var currentHash = Convert.ToBase64String(hashBytes);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(currentHash),
            System.Text.Encoding.UTF8.GetBytes(expectedHash));
    }
}
