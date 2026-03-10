using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class AuditRepository : IAuditRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuditRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAuditLogAsync(
        CreateAuditLogRequestDto request,
        Guid? userId,
        string? userEmail,
        string? userRoleCsv,
        string? ipAddress,
        string? userAgent,
        Guid? correlationId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_AuditLogs_Insert", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.AddWithValue("@AuditLogId", Guid.NewGuid());
        command.Parameters.AddWithValue("@EventType", request.EventType);
        command.Parameters.AddWithValue("@ActionName", request.ActionName);
        command.Parameters.AddWithValue("@EntityName", (object?)request.EntityName ?? DBNull.Value);
        command.Parameters.AddWithValue("@EntityId", (object?)request.EntityId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ClaimId", (object?)request.ClaimId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequestMethod", (object?)request.RequestMethod ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequestPath", (object?)request.RequestPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequestQuery", (object?)request.RequestQuery ?? DBNull.Value);
        command.Parameters.AddWithValue("@HttpStatusCode", (object?)request.HttpStatusCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsSuccess", request.IsSuccess);
        command.Parameters.AddWithValue("@DurationMs", (object?)request.DurationMs ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserEmail", (object?)userEmail ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserRoleCsv", (object?)userRoleCsv ?? DBNull.Value);
        command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserAgent", (object?)userAgent ?? DBNull.Value);
        command.Parameters.AddWithValue("@CorrelationId", (object?)(request.CorrelationId ?? correlationId) ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_AuditLogs_GetList", connection) { CommandType = CommandType.StoredProcedure };
        AddFilterParameters(command, filter);
        command.Parameters.AddWithValue("@Take", (object?)filter.Take ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var logs = new List<AuditLogDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            logs.Add(new AuditLogDto
            {
                AuditLogId = reader.GetGuid(reader.GetOrdinal("AuditLogId")),
                EventType = reader.GetString(reader.GetOrdinal("EventType")),
                ActionName = reader.GetString(reader.GetOrdinal("ActionName")),
                EntityName = GetNullableString(reader, "EntityName"),
                EntityId = GetNullableGuid(reader, "EntityId"),
                ClaimId = GetNullableGuid(reader, "ClaimId"),
                Description = GetNullableString(reader, "Description"),
                RequestMethod = GetNullableString(reader, "RequestMethod"),
                RequestPath = GetNullableString(reader, "RequestPath"),
                RequestQuery = GetNullableString(reader, "RequestQuery"),
                HttpStatusCode = GetNullableInt(reader, "HttpStatusCode"),
                IsSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess")),
                DurationMs = GetNullableInt(reader, "DurationMs"),
                UserId = GetNullableGuid(reader, "UserId"),
                UserEmail = GetNullableString(reader, "UserEmail"),
                UserRoleCsv = GetNullableString(reader, "UserRoleCsv"),
                IpAddress = GetNullableString(reader, "IpAddress"),
                UserAgent = GetNullableString(reader, "UserAgent"),
                CorrelationId = GetNullableGuid(reader, "CorrelationId"),
                CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
            });
        }

        return logs;
    }

    public async Task<AuditReportSummaryDto> GetAuditReportAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_AuditLogs_GetReport", connection) { CommandType = CommandType.StoredProcedure };
        AddFilterParameters(command, filter);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AuditReportSummaryDto();
        }

        return new AuditReportSummaryDto
        {
            TotalEvents = ReadLong(reader, "TotalEvents"),
            SuccessfulEvents = ReadLong(reader, "SuccessfulEvents"),
            FailedEvents = ReadLong(reader, "FailedEvents"),
            UserActions = ReadLong(reader, "UserActions"),
            ClaimChanges = ReadLong(reader, "ClaimChanges"),
            ApiActivities = ReadLong(reader, "ApiActivities"),
            DistinctUsers = ReadLong(reader, "DistinctUsers"),
            DistinctClaims = ReadLong(reader, "DistinctClaims")
        };
    }

    private static void AddFilterParameters(SqlCommand command, AuditLogFilterDto filter)
    {
        command.Parameters.AddWithValue("@FromDateUtc", (object?)filter.FromDateUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@ToDateUtc", (object?)filter.ToDateUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@EventType", (object?)filter.EventType ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserId", (object?)filter.UserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ClaimId", (object?)filter.ClaimId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsSuccess", (object?)filter.IsSuccess ?? DBNull.Value);
        command.Parameters.AddWithValue("@ActionContains", (object?)filter.ActionContains ?? DBNull.Value);
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static Guid? GetNullableGuid(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetGuid(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static long ReadLong(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return 0;
        }

        return Convert.ToInt64(reader.GetValue(ordinal));
    }
}
