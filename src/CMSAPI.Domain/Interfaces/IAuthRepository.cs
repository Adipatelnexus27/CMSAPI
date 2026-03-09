using CMSAPI.Domain.Entities;

namespace CMSAPI.Domain.Interfaces;

public interface IAuthRepository
{
    Task<AuthUser?> GetUserByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default);
    Task<AuthUser?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AddUserAsync(AuthUser user, CancellationToken cancellationToken = default);
    void UpdateUser(AuthUser user);

    Task<AuthRole?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken = default);
    Task<AuthRole?> GetRoleByIdAsync(long roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthRole>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(long roleId, CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<AuthRefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    void UpdateRefreshToken(AuthRefreshToken refreshToken);
}

