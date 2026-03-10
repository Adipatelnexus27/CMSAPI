using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IClaimRepository
{
    Task<bool> ValidatePolicyAsync(string policyNumber, DateTime incidentDateUtc, CancellationToken cancellationToken);
    Task<string> GenerateClaimNumberAsync(CancellationToken cancellationToken);
    Task<ClaimSummaryDto> CreateClaimAsync(string claimNumber, CreateClaimRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<ClaimDetailDto?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RelatedClaimDto>> GetRelatedClaimsAsync(Guid claimId, CancellationToken cancellationToken);
    Task<ClaimDocumentDto> AddClaimDocumentAsync(Guid claimId, string originalFileName, string storedFilePath, string contentType, long fileSizeBytes, CancellationToken cancellationToken);
    Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken);
}
