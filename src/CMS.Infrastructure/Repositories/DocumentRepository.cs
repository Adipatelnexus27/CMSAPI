using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DocumentRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<DocumentRecordDto>> GetClaimDocumentsAsync(Guid claimId, bool latestOnly, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Documents_GetByClaimId", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@LatestOnly", latestOnly);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var documents = new List<DocumentRecordDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(MapDocument(reader));
        }

        return documents;
    }

    public async Task<DocumentRecordDto?> GetDocumentByIdAsync(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Documents_GetById", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimDocumentId", claimDocumentId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapDocument(reader);
    }

    public async Task<IReadOnlyList<DocumentRecordDto>> GetDocumentVersionsAsync(Guid documentGroupId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Documents_GetVersionsByGroup", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@DocumentGroupId", documentGroupId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var documents = new List<DocumentRecordDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(MapDocument(reader));
        }

        return documents;
    }

    public async Task<DocumentRecordDto> AddDocumentVersionAsync(
        Guid claimId,
        string originalFileName,
        string storedFilePath,
        string contentType,
        long fileSizeBytes,
        string documentCategory,
        Guid? documentGroupId,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Documents_AddVersion", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ClaimDocumentId", Guid.NewGuid());
        command.Parameters.AddWithValue("@ClaimId", claimId);
        command.Parameters.AddWithValue("@OriginalFileName", originalFileName);
        command.Parameters.AddWithValue("@StoredFilePath", storedFilePath);
        command.Parameters.AddWithValue("@ContentType", contentType);
        command.Parameters.AddWithValue("@FileSizeBytes", fileSizeBytes);
        command.Parameters.AddWithValue("@DocumentCategory", documentCategory);
        command.Parameters.AddWithValue("@DocumentGroupId", (object?)documentGroupId ?? DBNull.Value);
        command.Parameters.AddWithValue("@UploadedByUserId", (object?)uploadedByUserId ?? DBNull.Value);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Document record could not be created.");
        }

        return MapDocument(reader);
    }

    private static DocumentRecordDto MapDocument(SqlDataReader reader)
    {
        return new DocumentRecordDto
        {
            ClaimDocumentId = reader.GetGuid(reader.GetOrdinal("ClaimDocumentId")),
            ClaimId = reader.GetGuid(reader.GetOrdinal("ClaimId")),
            OriginalFileName = reader.GetString(reader.GetOrdinal("OriginalFileName")),
            StoredFilePath = reader.GetString(reader.GetOrdinal("StoredFilePath")),
            DocumentCategory = reader.IsDBNull(reader.GetOrdinal("DocumentCategory")) ? "General" : reader.GetString(reader.GetOrdinal("DocumentCategory")),
            ContentType = reader.GetString(reader.GetOrdinal("ContentType")),
            FileSizeBytes = reader.GetInt64(reader.GetOrdinal("FileSizeBytes")),
            UploadedAtUtc = reader.GetDateTime(reader.GetOrdinal("UploadedAtUtc")),
            DocumentGroupId = reader.GetGuid(reader.GetOrdinal("DocumentGroupId")),
            VersionNumber = reader.GetInt32(reader.GetOrdinal("VersionNumber")),
            IsLatest = reader.GetBoolean(reader.GetOrdinal("IsLatest")),
            UploadedByUserId = reader.IsDBNull(reader.GetOrdinal("UploadedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("UploadedByUserId"))
        };
    }
}
