using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class ReportingRepository : IReportingRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ReportingRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ClaimsByStatusReportDto>> GetClaimsByStatusAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = CreateReportCommand("sp_Reports_ClaimsByStatus", connection, fromDateUtc, toDateUtc);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimsByStatusReportDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ClaimsByStatusReportDto
            {
                ClaimStatus = reader.GetString(reader.GetOrdinal("ClaimStatus")),
                ClaimCount = reader.GetInt32(reader.GetOrdinal("ClaimCount")),
                PercentageOfTotal = reader.GetDecimal(reader.GetOrdinal("PercentageOfTotal"))
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<ClaimsByProductReportDto>> GetClaimsByProductAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = CreateReportCommand("sp_Reports_ClaimsByProduct", connection, fromDateUtc, toDateUtc);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<ClaimsByProductReportDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ClaimsByProductReportDto
            {
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ClaimCount = reader.GetInt32(reader.GetOrdinal("ClaimCount")),
                OpenClaims = reader.GetInt32(reader.GetOrdinal("OpenClaims")),
                ClosedClaims = reader.GetInt32(reader.GetOrdinal("ClosedClaims"))
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<FraudReportDto>> GetFraudReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = CreateReportCommand("sp_Reports_FraudSummary", connection, fromDateUtc, toDateUtc);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<FraudReportDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new FraudReportDto
            {
                FraudStatus = reader.GetString(reader.GetOrdinal("FraudStatus")),
                FlagCount = reader.GetInt32(reader.GetOrdinal("FlagCount")),
                DuplicateFlags = reader.GetInt32(reader.GetOrdinal("DuplicateFlags")),
                SuspiciousFlags = reader.GetInt32(reader.GetOrdinal("SuspiciousFlags")),
                AverageSeverityScore = reader.GetDecimal(reader.GetOrdinal("AverageSeverityScore"))
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<InvestigatorPerformanceReportDto>> GetInvestigatorPerformanceReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = CreateReportCommand("sp_Reports_InvestigatorPerformance", connection, fromDateUtc, toDateUtc);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<InvestigatorPerformanceReportDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new InvestigatorPerformanceReportDto
            {
                InvestigatorUserId = reader.GetGuid(reader.GetOrdinal("InvestigatorUserId")),
                InvestigatorName = reader.GetString(reader.GetOrdinal("InvestigatorName")),
                AssignedClaims = reader.GetInt32(reader.GetOrdinal("AssignedClaims")),
                ClosedClaims = reader.GetInt32(reader.GetOrdinal("ClosedClaims")),
                AverageInvestigationProgress = reader.GetDecimal(reader.GetOrdinal("AverageInvestigationProgress")),
                TotalNotes = reader.GetInt32(reader.GetOrdinal("TotalNotes")),
                FraudFlagsOnAssignedClaims = reader.GetInt32(reader.GetOrdinal("FraudFlagsOnAssignedClaims"))
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<SettlementReportDto>> GetSettlementReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = CreateReportCommand("sp_Reports_SettlementSummary", connection, fromDateUtc, toDateUtc);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<SettlementReportDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new SettlementReportDto
            {
                PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                PaymentCount = reader.GetInt32(reader.GetOrdinal("PaymentCount")),
                TotalPaymentAmount = reader.GetDecimal(reader.GetOrdinal("TotalPaymentAmount")),
                AveragePaymentAmount = reader.GetDecimal(reader.GetOrdinal("AveragePaymentAmount"))
            });
        }

        return rows;
    }

    private static SqlCommand CreateReportCommand(string procedureName, SqlConnection connection, DateTime? fromDateUtc, DateTime? toDateUtc)
    {
        var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@FromDateUtc", (object?)fromDateUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@ToDateUtc", (object?)toDateUtc ?? DBNull.Value);
        return command;
    }
}
