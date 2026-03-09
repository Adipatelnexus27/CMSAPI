using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Application.Security;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;
using CMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMSAPI.Infrastructure.Services;

public sealed class AuthDataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ILogger<AuthDataSeeder> _logger;

    public AuthDataSeeder(
        ApplicationDbContext dbContext,
        IPasswordHasherService passwordHasher,
        ILogger<AuthDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolePermissionsAsync(cancellationToken);
        await SeedDefaultAdminAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var rolesToSeed = new (string RoleCode, string RoleName, string Description)[]
        {
            (nameof(UserRole.Admin), "Admin", "System administrator"),
            (nameof(UserRole.ClaimManager), "Claim Manager", "Claims operations manager"),
            (nameof(UserRole.Investigator), "Investigator", "Claim investigation specialist"),
            (nameof(UserRole.Adjuster), "Adjuster", "Claim adjuster"),
            (nameof(UserRole.Finance), "Finance", "Payments and reconciliation"),
            (nameof(UserRole.FraudAnalyst), "Fraud Analyst", "Fraud scoring and review")
        };

        foreach (var role in rolesToSeed)
        {
            var exists = await _dbContext.AuthRoles.AnyAsync(x => x.RoleCode == role.RoleCode, cancellationToken);
            if (exists)
            {
                continue;
            }

            _dbContext.AuthRoles.Add(new AuthRole
            {
                RoleCode = role.RoleCode,
                RoleName = role.RoleName,
                RoleDescription = role.Description,
                CreatedBy = "seed"
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        foreach (var permission in Permissions.All)
        {
            var exists = await _dbContext.AuthPermissions.AnyAsync(x => x.PermissionCode == permission, cancellationToken);
            if (exists)
            {
                continue;
            }

            _dbContext.AuthPermissions.Add(new AuthPermission
            {
                PermissionCode = permission,
                PermissionName = permission,
                PermissionDescription = $"Permission for {permission}",
                CreatedBy = "seed"
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolePermissionsAsync(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.AuthRoles.AsNoTracking().ToDictionaryAsync(x => x.RoleCode, x => x.RoleId, cancellationToken);
        var permissions = await _dbContext.AuthPermissions.AsNoTracking().ToDictionaryAsync(x => x.PermissionCode, x => x.PermissionId, cancellationToken);

        var mappings = new Dictionary<string, string[]>
        {
            [nameof(UserRole.Admin)] = Permissions.All,
            [nameof(UserRole.ClaimManager)] =
            [
                Permissions.ClaimsRead,
                Permissions.ClaimsCreate,
                Permissions.ClaimsAssign,
                Permissions.ClaimsAdjudicate
            ],
            [nameof(UserRole.Investigator)] =
            [
                Permissions.ClaimsRead,
                Permissions.ClaimsInvestigate
            ],
            [nameof(UserRole.Adjuster)] =
            [
                Permissions.ClaimsRead,
                Permissions.ClaimsCreate
            ],
            [nameof(UserRole.Finance)] =
            [
                Permissions.ClaimsRead,
                Permissions.ClaimsPay
            ],
            [nameof(UserRole.FraudAnalyst)] =
            [
                Permissions.ClaimsRead,
                Permissions.FraudReview
            ]
        };

        foreach (var mapping in mappings)
        {
            if (!roles.TryGetValue(mapping.Key, out var roleId))
            {
                continue;
            }

            foreach (var permissionCode in mapping.Value)
            {
                if (!permissions.TryGetValue(permissionCode, out var permissionId))
                {
                    continue;
                }

                var exists = await _dbContext.AuthRolePermissions.AnyAsync(
                    x => x.RoleId == roleId && x.PermissionId == permissionId,
                    cancellationToken);
                if (exists)
                {
                    continue;
                }

                _dbContext.AuthRolePermissions.Add(new AuthRolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedBy = "seed"
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDefaultAdminAsync(CancellationToken cancellationToken)
    {
        var adminRole = await _dbContext.AuthRoles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoleCode == nameof(UserRole.Admin), cancellationToken);

        if (adminRole is null)
        {
            _logger.LogWarning("Admin role was not found during auth seed.");
            return;
        }

        var adminExists = await _dbContext.AuthUsers.AnyAsync(x => x.UserName == "admin", cancellationToken);
        if (adminExists)
        {
            return;
        }

        var (hash, salt) = _passwordHasher.HashPassword("Admin@123");
        _dbContext.AuthUsers.Add(new AuthUser
        {
            UserName = "admin",
            DisplayName = "System Admin",
            Email = "admin@cms.local",
            RoleId = adminRole.RoleId,
            PasswordHash = hash,
            PasswordSalt = salt,
            LastPasswordChangedDate = DateTime.UtcNow,
            CreatedBy = "seed"
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

