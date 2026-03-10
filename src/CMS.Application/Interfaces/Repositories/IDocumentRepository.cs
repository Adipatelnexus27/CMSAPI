using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentRecordDto>> GetClaimDocumentsAsync(Guid claimId, bool latestOnly, CancellationToken cancellationToken);
    Task<DocumentRecordDto?> GetDocumentByIdAsync(Guid claimDocumentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentRecordDto>> GetDocumentVersionsAsync(Guid documentGroupId, CancellationToken cancellationToken);
    Task<DocumentRecordDto> AddDocumentVersionAsync(
        Guid claimId,
        string originalFileName,
        string storedFilePath,
        string contentType,
        long fileSizeBytes,
        string documentCategory,
        Guid? documentGroupId,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken);
}
