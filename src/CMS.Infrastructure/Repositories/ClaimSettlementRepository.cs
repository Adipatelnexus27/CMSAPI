using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class ClaimSettlementRepository : IClaimSettlementRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClaimSettlementRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ClaimSettlementDto> CalculateSettlementAsync(Guid claimId, decimal grossLossAmount, string currencyCode, Guid? calculatedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_Calculate", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@GrossLossAmount", grossLossAmount);
        command.Parameters.AddWithValue("@CurrencyCode", currencyCode);
        command.Parameters.AddWithValue("@CalculatedByUserId", (object?)calculatedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Unable to calculate settlement.");
        }

        return MapSettlement(reader);
    }

    public async Task<ClaimSettlementDto?> GetSettlementByClaimIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_GetByClaimId", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapSettlement(reader);
    }

    public async Task<ClaimPaymentDto> RequestPaymentApprovalAsync(Guid claimSettlementId, decimal paymentAmount, string? requestNote, Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_RequestPayment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimSettlementId", claimSettlementId);
        command.Parameters.AddWithValue("@PaymentAmount", paymentAmount);
        command.Parameters.AddWithValue("@RequestNote", (object?)requestNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequestedByUserId", (object?)requestedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Unable to request payment approval.");
        }

        return MapPayment(reader);
    }

    public async Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsAsync(string? paymentStatus, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_GetPayments", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", DBNull.Value);
        command.Parameters.AddWithValue("@PaymentStatus", (object?)paymentStatus ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimPaymentDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(MapPayment(reader));
        }

        return rows;
    }

    public async Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_GetPayments", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@PaymentStatus", DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimPaymentDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(MapPayment(reader));
        }

        return rows;
    }

    public async Task ApprovePaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_ApprovePayment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimPaymentId", claimPaymentId);
        command.Parameters.AddWithValue("@ApprovalNote", (object?)approvalNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ApprovedByUserId", (object?)approvedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RejectPaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_RejectPayment", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimPaymentId", claimPaymentId);
        command.Parameters.AddWithValue("@ApprovalNote", (object?)approvalNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ApprovedByUserId", (object?)approvedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdatePaymentStatusAsync(Guid claimPaymentId, string paymentStatus, string? statusNote, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_UpdatePaymentStatus", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimPaymentId", claimPaymentId);
        command.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
        command.Parameters.AddWithValue("@StatusNote", (object?)statusNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChangedByUserId", (object?)changedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimPaymentStatusHistoryDto>> GetPaymentStatusHistoryAsync(Guid claimPaymentId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Settlement_GetPaymentStatusHistory", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimPaymentId", claimPaymentId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimPaymentStatusHistoryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ClaimPaymentStatusHistoryDto
            {
                ClaimPaymentStatusHistoryId = reader.GetGuid(reader.GetOrdinal("ClaimPaymentStatusHistoryId")),
                ClaimPaymentId = reader.GetGuid(reader.GetOrdinal("ClaimPaymentId")),
                PreviousStatus = reader.IsDBNull(reader.GetOrdinal("PreviousStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("PreviousStatus")),
                NewStatus = reader.GetString(reader.GetOrdinal("NewStatus")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note")),
                ChangedByUserId = reader.IsDBNull(reader.GetOrdinal("ChangedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ChangedByUserId")),
                ChangedAtUtc = reader.GetDateTime(reader.GetOrdinal("ChangedAtUtc"))
            });
        }

        return rows;
    }

    private static ClaimSettlementDto MapSettlement(SqlDataReader reader)
    {
        return new ClaimSettlementDto
        {
            ClaimSettlementId = reader.GetGuid(reader.GetOrdinal("ClaimSettlementId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            GrossLossAmount = reader.GetDecimal(reader.GetOrdinal("GrossLossAmount")),
            PolicyLimitAmount = reader.GetDecimal(reader.GetOrdinal("PolicyLimitAmount")),
            DeductibleAmount = reader.GetDecimal(reader.GetOrdinal("DeductibleAmount")),
            EligibleAmount = reader.GetDecimal(reader.GetOrdinal("EligibleAmount")),
            ApprovedSettlementAmount = reader.GetDecimal(reader.GetOrdinal("ApprovedSettlementAmount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            CalculationStatus = reader.GetString(reader.GetOrdinal("CalculationStatus")),
            CalculatedByUserId = reader.IsDBNull(reader.GetOrdinal("CalculatedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("CalculatedByUserId")),
            CalculatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CalculatedAtUtc")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc"))
        };
    }

    private static ClaimPaymentDto MapPayment(SqlDataReader reader)
    {
        return new ClaimPaymentDto
        {
            ClaimPaymentId = reader.GetGuid(reader.GetOrdinal("ClaimPaymentId")),
            ClaimSettlementId = reader.GetGuid(reader.GetOrdinal("ClaimSettlementId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            PaymentAmount = reader.GetDecimal(reader.GetOrdinal("PaymentAmount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
            RequestNote = reader.IsDBNull(reader.GetOrdinal("RequestNote")) ? null : reader.GetString(reader.GetOrdinal("RequestNote")),
            RequestedByUserId = reader.IsDBNull(reader.GetOrdinal("RequestedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("RequestedByUserId")),
            RequestedAtUtc = reader.GetDateTime(reader.GetOrdinal("RequestedAtUtc")),
            ApprovedByUserId = reader.IsDBNull(reader.GetOrdinal("ApprovedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ApprovedByUserId")),
            ApprovedAtUtc = reader.IsDBNull(reader.GetOrdinal("ApprovedAtUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedAtUtc")),
            ApprovalNote = reader.IsDBNull(reader.GetOrdinal("ApprovalNote")) ? null : reader.GetString(reader.GetOrdinal("ApprovalNote")),
            StatusNote = reader.IsDBNull(reader.GetOrdinal("StatusNote")) ? null : reader.GetString(reader.GetOrdinal("StatusNote")),
            LastStatusUpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("LastStatusUpdatedAtUtc"))
        };
    }
}
