using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IClaimService
{
    Task<ClaimSummaryDto> RegisterClaimAsync(CreateClaimRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<ClaimDetailDto> GetClaimDetailAsync(Guid claimId, CancellationToken cancellationToken);
    Task<UploadClaimDocumentResponseDto> UploadDocumentAsync(Guid claimId, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, CancellationToken cancellationToken);
    Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken);
}
