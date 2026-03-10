using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IClaimService
{
    Task<ClaimSummaryDto> RegisterClaimAsync(CreateClaimRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetAssignedClaimsAsync(Guid assigneeUserId, string role, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetInvestigationDashboardAsync(CancellationToken cancellationToken);
    Task<ClaimDetailDto> GetClaimDetailAsync(Guid claimId, CancellationToken cancellationToken);
    Task<ClaimInvestigationDto> GetClaimInvestigationAsync(Guid claimId, CancellationToken cancellationToken);
    Task<UploadClaimDocumentResponseDto> UploadDocumentAsync(Guid claimId, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, Guid? uploadedByUserId, CancellationToken cancellationToken);
    Task<UploadClaimDocumentResponseDto> UploadInvestigationDocumentAsync(Guid claimId, string documentCategory, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, Guid? uploadedByUserId, CancellationToken cancellationToken);
    Task<InvestigationNoteDto> AddInvestigatorNoteAsync(Guid claimId, string noteText, int? progressPercentSnapshot, Guid? createdByUserId, CancellationToken cancellationToken);
    Task UpdateInvestigationProgressAsync(Guid claimId, int progressPercent, Guid? changedByUserId, CancellationToken cancellationToken);
    Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken);
    Task AssignInvestigatorAsync(Guid claimId, Guid investigatorUserId, Guid? changedByUserId, CancellationToken cancellationToken);
    Task AssignAdjusterAsync(Guid claimId, Guid adjusterUserId, Guid? changedByUserId, CancellationToken cancellationToken);
    Task SetPriorityAsync(Guid claimId, int priority, Guid? changedByUserId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid claimId, string claimStatus, Guid? changedByUserId, CancellationToken cancellationToken);
    Task UpdateWorkflowStepAsync(Guid claimId, string workflowStep, Guid? changedByUserId, CancellationToken cancellationToken);
}
