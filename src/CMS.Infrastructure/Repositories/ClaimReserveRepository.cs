using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class ClaimReserveRepository : IClaimReserveRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClaimReserveRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ClaimReserveDto> CreateInitialReserveAsync(Guid claimId, decimal reserveAmount, string currencyCode, string? reason, Guid? createdByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_CreateInitial", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@ReserveAmount", reserveAmount);
        command.Parameters.AddWithValue("@CurrencyCode", currencyCode);
        command.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedByUserId", (object?)createdByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Unable to create initial reserve.");
        }

        return MapReserve(reader);
    }

    public async Task<ClaimReserveHistoryDto> RequestReserveAdjustmentAsync(Guid claimId, decimal reserveAmount, string? reason, Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_RequestAdjustment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@ReserveAmount", reserveAmount);
        command.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequestedByUserId", (object?)requestedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Unable to request reserve adjustment.");
        }

        return MapReserveHistory(reader);
    }

    public async Task<ClaimReserveDto?> GetClaimReserveAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_GetByClaimId", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapReserve(reader);
    }

    public async Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveHistoryAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_GetHistory", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimReserveHistoryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(MapReserveHistory(reader));
        }

        return rows;
    }

    public async Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveApprovalQueueAsync(string? status, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_GetApprovalQueue", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimReserveHistoryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(MapReserveHistory(reader));
        }

        return rows;
    }

    public async Task ApproveReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_ApproveAdjustment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimReserveHistoryId", claimReserveHistoryId);
        command.Parameters.AddWithValue("@ApprovalNote", (object?)approvalNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ApprovedByUserId", (object?)approvedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RejectReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Reserve_RejectAdjustment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimReserveHistoryId", claimReserveHistoryId);
        command.Parameters.AddWithValue("@ApprovalNote", (object?)approvalNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ApprovedByUserId", (object?)approvedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ClaimReserveDto MapReserve(SqlDataReader reader)
    {
        return new ClaimReserveDto
        {
            ClaimReserveId = reader.GetGuid(reader.GetOrdinal("ClaimReserveId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            CurrentReserveAmount = reader.GetDecimal(reader.GetOrdinal("CurrentReserveAmount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            LastApprovedAtUtc = reader.IsDBNull(reader.GetOrdinal("LastApprovedAtUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("LastApprovedAtUtc")),
            LastApprovedByUserId = reader.IsDBNull(reader.GetOrdinal("LastApprovedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("LastApprovedByUserId")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc"))
        };
    }

    private static ClaimReserveHistoryDto MapReserveHistory(SqlDataReader reader)
    {
        return new ClaimReserveHistoryDto
        {
            ClaimReserveHistoryId = reader.GetGuid(reader.GetOrdinal("ClaimReserveHistoryId")),
            ClaimReserveId = reader.GetGuid(reader.GetOrdinal("ClaimReserveId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            ActionType = reader.GetString(reader.GetOrdinal("ActionType")),
            PreviousReserveAmount = reader.IsDBNull(reader.GetOrdinal("PreviousReserveAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("PreviousReserveAmount")),
            RequestedReserveAmount = reader.GetDecimal(reader.GetOrdinal("RequestedReserveAmount")),
            ApprovedReserveAmount = reader.IsDBNull(reader.GetOrdinal("ApprovedReserveAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("ApprovedReserveAmount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
            RequestedByUserId = reader.IsDBNull(reader.GetOrdinal("RequestedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("RequestedByUserId")),
            RequestedAtUtc = reader.GetDateTime(reader.GetOrdinal("RequestedAtUtc")),
            ApprovedByUserId = reader.IsDBNull(reader.GetOrdinal("ApprovedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ApprovedByUserId")),
            ApprovedAtUtc = reader.IsDBNull(reader.GetOrdinal("ApprovedAtUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedAtUtc")),
            ApprovalNote = reader.IsDBNull(reader.GetOrdinal("ApprovalNote")) ? null : reader.GetString(reader.GetOrdinal("ApprovalNote"))
        };
    }
}
