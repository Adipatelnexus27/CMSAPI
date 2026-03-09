using CMSAPI.Domain.Entities;

namespace CMSAPI.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(AuthUser user, string roleCode, IReadOnlyCollection<string> permissions, DateTime expiresAtUtc);
    string GenerateRefreshToken();
    string HashToken(string token);
}

