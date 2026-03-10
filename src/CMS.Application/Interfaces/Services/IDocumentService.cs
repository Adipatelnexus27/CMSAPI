using CMS.Application.DTOs;
using CMS.Application.Models;

namespace CMS.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<UploadDocumentResponseDto> UploadDocumentAsync(
        Guid claimId,
        string? documentCategory,
        Guid? documentGroupId,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        Stream contentStream,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId, bool latestOnly, CancellationToken cancellationToken);
    Task<ClaimDocumentDto> GetDocumentAsync(Guid claimDocumentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimDocumentDto>> GetDocumentVersionsAsync(Guid claimDocumentId, CancellationToken cancellationToken);
    Task<DocumentPreviewResult> GetDocumentPreviewAsync(Guid claimDocumentId, CancellationToken cancellationToken);
}
