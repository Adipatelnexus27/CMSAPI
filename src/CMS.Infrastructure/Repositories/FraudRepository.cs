using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class FraudRepository : IFraudRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public FraudRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<FraudFlagDto> CreateOrReuseFraudFlagAsync(
        Guid claimId,
        string flagType,
        string? ruleName,
        int severityScore,
        string reason,
        bool isDuplicate,
        bool isSuspicious,
        Guid? createdByUserId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Fraud_CreateOrReuseFlag", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@FlagType", flagType);
        command.Parameters.AddWithValue("@RuleName", (object?)ruleName ?? DBNull.Value);
        command.Parameters.AddWithValue("@SeverityScore", severityScore);
        command.Parameters.AddWithValue("@Reason", reason);
        command.Parameters.AddWithValue("@IsDuplicate", isDuplicate);
        command.Parameters.AddWithValue("@IsSuspicious", isSuspicious);
        command.Parameters.AddWithValue("@CreatedByUserId", (object?)createdByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Unable to create fraud flag.");
        }

        return MapFraudFlag(reader);
    }

    public async Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsAsync(string? status, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Fraud_GetFlags", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", DBNull.Value);
        command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim());

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var flags = new List<FraudFlagDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            flags.Add(MapFraudFlag(reader));
        }

        return flags;
    }

    public async Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Fraud_GetFlags", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@Status", DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var flags = new List<FraudFlagDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            flags.Add(MapFraudFlag(reader));
        }

        return flags;
    }

    public async Task UpdateFraudFlagStatusAsync(Guid fraudFlagId, string status, string? reviewNote, Guid? reviewedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Fraud_UpdateFlagStatus", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@FraudFlagId", fraudFlagId);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@ReviewNote", (object?)reviewNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReviewedByUserId", (object?)reviewedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static FraudFlagDto MapFraudFlag(SqlDataReader reader)
    {
        return new FraudFlagDto
        {
            FraudFlagId = reader.GetGuid(reader.GetOrdinal("FraudFlagId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            FlagType = reader.GetString(reader.GetOrdinal("FlagType")),
            RuleName = reader.IsDBNull(reader.GetOrdinal("RuleName")) ? null : reader.GetString(reader.GetOrdinal("RuleName")),
            SeverityScore = reader.GetInt32(reader.GetOrdinal("SeverityScore")),
            Reason = reader.GetString(reader.GetOrdinal("Reason")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            IsDuplicate = reader.GetBoolean(reader.GetOrdinal("IsDuplicate")),
            IsSuspicious = reader.GetBoolean(reader.GetOrdinal("IsSuspicious")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            CreatedByUserId = reader.IsDBNull(reader.GetOrdinal("CreatedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),
            ReviewedByUserId = reader.IsDBNull(reader.GetOrdinal("ReviewedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ReviewedByUserId")),
            ReviewedAtUtc = reader.IsDBNull(reader.GetOrdinal("ReviewedAtUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAtUtc")),
            ReviewNote = reader.IsDBNull(reader.GetOrdinal("ReviewNote")) ? null : reader.GetString(reader.GetOrdinal("ReviewNote"))
        };
    }
}
