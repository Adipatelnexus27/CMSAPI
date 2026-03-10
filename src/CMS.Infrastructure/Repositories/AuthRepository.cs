using System.Data;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Models;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuthRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AuthUserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetUserByEmail", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@Email", email);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return MapUser(reader);
    }

    public async Task<AuthUserRecord?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetUserById", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return MapUser(reader);
    }

    public async Task<IReadOnlyList<AuthUserListRecord>> GetUsersAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetUsers", connection) { CommandType = CommandType.StoredProcedure };
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<AuthUserListRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var rolesCsv = reader.IsDBNull(reader.GetOrdinal("RolesCsv"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("RolesCsv"));

            var roles = rolesCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            results.Add(new AuthUserListRecord
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Roles = roles,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
            });
        }

        return results;
    }

    public async Task RegisterUserAsync(Guid userId, string email, string fullName, string passwordHash, string passwordSalt, string role, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_RegisterUser", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@FullName", fullName);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
        command.Parameters.AddWithValue("@RoleName", role);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetUserRoles", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var roles = new List<string>();
        while (await reader.ReadAsync(cancellationToken)) roles.Add(reader.GetString(0));
        return roles;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetUserPermissions", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var permissions = new List<string>();
        while (await reader.ReadAsync(cancellationToken)) permissions.Add(reader.GetString(0));
        return permissions;
    }

    public async Task StoreRefreshTokenAsync(Guid refreshTokenId, Guid userId, string tokenHash, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_StoreRefreshToken", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@RefreshTokenId", refreshTokenId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@TokenHash", tokenHash);
        command.Parameters.AddWithValue("@ExpiresAtUtc", expiresAtUtc);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<RefreshTokenRecord?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_GetRefreshToken", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@TokenHash", tokenHash);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new RefreshTokenRecord
        {
            RefreshTokenId = reader.GetGuid(reader.GetOrdinal("RefreshTokenId")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            TokenHash = reader.GetString(reader.GetOrdinal("TokenHash")),
            ExpiresAtUtc = reader.GetDateTime(reader.GetOrdinal("ExpiresAtUtc")),
            RevokedAtUtc = reader.IsDBNull(reader.GetOrdinal("RevokedAtUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("RevokedAtUtc"))
        };
    }

    public async Task RevokeRefreshTokenAsync(string tokenHash, string reason, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Auth_RevokeRefreshToken", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@TokenHash", tokenHash);
        command.Parameters.AddWithValue("@Reason", reason);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static AuthUserRecord MapUser(SqlDataReader reader)
    {
        return new AuthUserRecord
        {
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            PasswordSalt = reader.GetString(reader.GetOrdinal("PasswordSalt")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }
}
