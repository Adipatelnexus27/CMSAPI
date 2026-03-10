using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClaimRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ValidatePolicyAsync(string policyNumber, DateTime incidentDateUtc, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_ValidatePolicy", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@PolicyNumber", policyNumber);
        command.Parameters.AddWithValue("@IncidentDateUtc", incidentDateUtc);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result ?? 0) == 1;
    }

    public async Task<string> GenerateClaimNumberAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GenerateClaimNumber", connection) { CommandType = CommandType.StoredProcedure };

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result?.ToString() ?? throw new InvalidOperationException("Unable to generate claim number.");
    }

    public async Task<ClaimSummaryDto> CreateClaimAsync(string claimNumber, CreateClaimRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_Create", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.AddWithValue("@ClaimId", Guid.NewGuid());
        command.Parameters.AddWithValue("@ClaimNumber", claimNumber);
        command.Parameters.AddWithValue("@PolicyNumber", request.PolicyNumber);
        command.Parameters.AddWithValue("@ClaimType", request.ClaimType);
        command.Parameters.AddWithValue("@ReporterName", request.ReporterName);
        command.Parameters.AddWithValue("@IncidentDateUtc", request.IncidentDateUtc);
        command.Parameters.AddWithValue("@IncidentLocation", request.IncidentLocation);
        command.Parameters.AddWithValue("@IncidentDescription", request.IncidentDescription);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Claim could not be created.");
        }

        return MapSummary(reader);
    }

    public async Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetList", connection) { CommandType = CommandType.StoredProcedure };

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var claims = new List<ClaimSummaryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            claims.Add(MapSummary(reader));
        }

        return claims;
    }

    public async Task<IReadOnlyList<ClaimSummaryDto>> GetAssignedClaimsAsync(Guid assigneeUserId, string role, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetAssignedClaims", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@AssigneeUserId", assigneeUserId);
        command.Parameters.AddWithValue("@Role", role);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var claims = new List<ClaimSummaryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            claims.Add(MapSummary(reader));
        }

        return claims;
    }

    public async Task<ClaimDetailDto?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetById", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ClaimDetailDto
        {
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            PolicyNumber = reader.GetString(reader.GetOrdinal("PolicyNumber")),
            ClaimType = reader.GetString(reader.GetOrdinal("ClaimType")),
            ClaimStatus = reader.GetString(reader.GetOrdinal("ClaimStatus")),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            WorkflowStep = reader.GetString(reader.GetOrdinal("WorkflowStep")),
            InvestigatorUserId = reader.IsDBNull(reader.GetOrdinal("InvestigatorUserId")) ? null : reader.GetGuid(reader.GetOrdinal("InvestigatorUserId")),
            AdjusterUserId = reader.IsDBNull(reader.GetOrdinal("AdjusterUserId")) ? null : reader.GetGuid(reader.GetOrdinal("AdjusterUserId")),
            ReporterName = reader.GetString(reader.GetOrdinal("ReporterName")),
            IncidentDateUtc = reader.GetDateTime(reader.GetOrdinal("IncidentDateUtc")),
            IncidentLocation = reader.GetString(reader.GetOrdinal("IncidentLocation")),
            IncidentDescription = reader.GetString(reader.GetOrdinal("IncidentDescription")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
    }

    public async Task<IReadOnlyList<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetDocuments", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var documents = new List<ClaimDocumentDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(new ClaimDocumentDto
            {
                ClaimDocumentId = reader.GetGuid(reader.GetOrdinal("ClaimDocumentId")),
                ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
                OriginalFileName = reader.GetString(reader.GetOrdinal("OriginalFileName")),
                ContentType = reader.GetString(reader.GetOrdinal("ContentType")),
                FileSizeBytes = reader.GetInt64(reader.GetOrdinal("FileSizeBytes")),
                UploadedAtUtc = reader.GetDateTime(reader.GetOrdinal("UploadedAtUtc"))
            });
        }

        return documents;
    }

    public async Task<IReadOnlyList<RelatedClaimDto>> GetRelatedClaimsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetRelatedClaims", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var claims = new List<RelatedClaimDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            claims.Add(new RelatedClaimDto
            {
                ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
                ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
                ClaimStatus = reader.GetString(reader.GetOrdinal("ClaimStatus"))
            });
        }

        return claims;
    }

    public async Task<IReadOnlyList<ClaimWorkflowHistoryDto>> GetWorkflowHistoryAsync(Guid claimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_GetWorkflowHistory", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var history = new List<ClaimWorkflowHistoryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            history.Add(new ClaimWorkflowHistoryDto
            {
                ClaimWorkflowHistoryId = reader.GetGuid(reader.GetOrdinal("ClaimWorkflowHistoryId")),
                ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
                ActionType = reader.GetString(reader.GetOrdinal("ActionType")),
                PreviousValue = reader.IsDBNull(reader.GetOrdinal("PreviousValue")) ? null : reader.GetString(reader.GetOrdinal("PreviousValue")),
                NewValue = reader.GetString(reader.GetOrdinal("NewValue")),
                ChangedByUserId = reader.IsDBNull(reader.GetOrdinal("ChangedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ChangedByUserId")),
                ChangedAtUtc = reader.GetDateTime(reader.GetOrdinal("ChangedAtUtc"))
            });
        }

        return history;
    }

    public async Task<ClaimDocumentDto> AddClaimDocumentAsync(Guid claimId, string originalFileName, string storedFilePath, string contentType, long fileSizeBytes, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_AddDocument", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.AddWithValue("@ClaimDocumentId", Guid.NewGuid());
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@OriginalFileName", originalFileName);
        command.Parameters.AddWithValue("@StoredFilePath", storedFilePath);
        command.Parameters.AddWithValue("@ContentType", contentType);
        command.Parameters.AddWithValue("@FileSizeBytes", fileSizeBytes);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Document record could not be created.");
        }

        return new ClaimDocumentDto
        {
            ClaimDocumentId = reader.GetGuid(reader.GetOrdinal("ClaimDocumentId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            OriginalFileName = reader.GetString(reader.GetOrdinal("OriginalFileName")),
            ContentType = reader.GetString(reader.GetOrdinal("ContentType")),
            FileSizeBytes = reader.GetInt64(reader.GetOrdinal("FileSizeBytes")),
            UploadedAtUtc = reader.GetDateTime(reader.GetOrdinal("UploadedAtUtc"))
        };
    }

    public async Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_LinkRelated", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@RelatedClaimId", relatedClaimId);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AssignInvestigatorAsync(Guid claimId, Guid investigatorUserId, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        await ExecuteClaimUpdateAsync("sp_Claims_AssignInvestigator", claimId, investigatorUserId, changedByUserId, cancellationToken, "@InvestigatorUserId");
    }

    public async Task AssignAdjusterAsync(Guid claimId, Guid adjusterUserId, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        await ExecuteClaimUpdateAsync("sp_Claims_AssignAdjuster", claimId, adjusterUserId, changedByUserId, cancellationToken, "@AdjusterUserId");
    }

    public async Task SetPriorityAsync(Guid claimId, int priority, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_SetPriority", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@Priority", priority);
        command.Parameters.AddWithValue("@ChangedByUserId", (object?)changedByUserId ?? DBNull.Value);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid claimId, string claimStatus, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_UpdateStatus", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@ClaimStatus", claimStatus);
        command.Parameters.AddWithValue("@ChangedByUserId", (object?)changedByUserId ?? DBNull.Value);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateWorkflowStepAsync(Guid claimId, string workflowStep, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Claims_UpdateWorkflowStep", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@WorkflowStep", workflowStep);
        command.Parameters.AddWithValue("@ChangedByUserId", (object?)changedByUserId ?? DBNull.Value);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ExecuteClaimUpdateAsync(string procedureName, Guid claimId, Guid assigneeUserId, Guid? changedByUserId, CancellationToken cancellationToken, string parameterName)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue(parameterName, assigneeUserId);
        command.Parameters.AddWithValue("@ChangedByUserId", (object?)changedByUserId ?? DBNull.Value);
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ClaimSummaryDto MapSummary(SqlDataReader reader)
    {
        return new ClaimSummaryDto
        {
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            ClaimNumber = reader.GetString(reader.GetOrdinal("ClaimNumber")),
            PolicyNumber = reader.GetString(reader.GetOrdinal("PolicyNumber")),
            ClaimType = reader.GetString(reader.GetOrdinal("ClaimType")),
            ClaimStatus = reader.GetString(reader.GetOrdinal("ClaimStatus")),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            WorkflowStep = reader.GetString(reader.GetOrdinal("WorkflowStep")),
            InvestigatorUserId = reader.IsDBNull(reader.GetOrdinal("InvestigatorUserId")) ? null : reader.GetGuid(reader.GetOrdinal("InvestigatorUserId")),
            AdjusterUserId = reader.IsDBNull(reader.GetOrdinal("AdjusterUserId")) ? null : reader.GetGuid(reader.GetOrdinal("AdjusterUserId")),
            ReporterName = reader.GetString(reader.GetOrdinal("ReporterName")),
            IncidentDateUtc = reader.GetDateTime(reader.GetOrdinal("IncidentDateUtc")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
    }
}
