using CMS.Application.Models;

namespace CMS.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<AuthUserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<AuthUserRecord?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task RegisterUserAsync(Guid userId, string email, string fullName, string passwordHash, string passwordSalt, string role, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task StoreRefreshTokenAsync(Guid refreshTokenId, Guid userId, string tokenHash, DateTime expiresAtUtc, CancellationToken cancellationToken);
    Task<RefreshTokenRecord?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string tokenHash, string reason, CancellationToken cancellationToken);
}
