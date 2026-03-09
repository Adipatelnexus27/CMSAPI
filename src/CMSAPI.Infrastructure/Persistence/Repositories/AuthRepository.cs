using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuthRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthUser?> GetUserByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthUsers
            .FirstOrDefaultAsync(
                x => x.UserName == userNameOrEmail || x.Email == userNameOrEmail,
                cancellationToken);
    }

    public async Task<AuthUser?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthUsers.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthUsers.AnyAsync(x => x.UserName == userName, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthUsers.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddUserAsync(AuthUser user, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthUsers.AddAsync(user, cancellationToken);
    }

    public void UpdateUser(AuthUser user)
    {
        _dbContext.AuthUsers.Update(user);
    }

    public async Task<AuthRole?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoleCode == roleCode && x.IsActive, cancellationToken);
    }

    public async Task<AuthRole?> GetRoleByIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthRole>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthRoles
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        return await (
            from rp in _dbContext.AuthRolePermissions.AsNoTracking()
            join p in _dbContext.AuthPermissions.AsNoTracking() on rp.PermissionId equals p.PermissionId
            where rp.RoleId == roleId && rp.IsActive && p.IsActive
            select p.PermissionCode
        ).Distinct().ToListAsync(cancellationToken);
    }

    public async Task AddRefreshTokenAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthRefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<AuthRefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuthRefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public void UpdateRefreshToken(AuthRefreshToken refreshToken)
    {
        _dbContext.AuthRefreshTokens.Update(refreshToken);
    }
}

