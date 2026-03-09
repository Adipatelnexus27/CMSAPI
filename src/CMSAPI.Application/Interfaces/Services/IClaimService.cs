using CMSAPI.Application.DTOs.Claims;

namespace CMSAPI.Application.Interfaces.Services;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClaimDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ClaimDto> CreateAsync(CreateClaimRequestDto request, long createdByUserId, string createdBy, CancellationToken cancellationToken = default);
    Task<ClaimDto> UpdateStatusAsync(long id, UpdateClaimStatusRequestDto request, string modifiedBy, CancellationToken cancellationToken = default);
    Task<ClaimDocumentDto> UploadDocumentAsync(long claimId, UploadClaimDocumentRequestDto request, long uploadedByUserId, string uploadedBy, CancellationToken cancellationToken = default);
    Task LinkRelatedClaimAsync(long claimId, long relatedClaimId, long userId, string userName, CancellationToken cancellationToken = default);
}
