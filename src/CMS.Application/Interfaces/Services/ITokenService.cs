using CMS.Application.Models;

namespace CMS.Application.Interfaces.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Guid userId, string email, string fullName, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
    GeneratedRefreshToken GenerateRefreshToken();
}
