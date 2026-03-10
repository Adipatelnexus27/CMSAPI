using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IClaimRepository
{
    Task<bool> ValidatePolicyAsync(string policyNumber, DateTime incidentDateUtc, CancellationToken cancellationToken);
    Task<string> GenerateClaimNumberAsync(CancellationToken cancellationToken);
    Task<ClaimSummaryDto> CreateClaimAsync(string claimNumber, CreateClaimRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimSummaryDto>> GetAssignedClaimsAsync(Guid assigneeUserId, string role, CancellationToken cancellationToken);
    Task<ClaimDetailDto?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimDocumentDto>> GetInvestigationDocumentsAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RelatedClaimDto>> GetRelatedClaimsAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimWorkflowHistoryDto>> GetWorkflowHistoryAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvestigationNoteDto>> GetInvestigationNotesAsync(Guid claimId, CancellationToken cancellationToken);
    Task<ClaimDocumentDto> AddClaimDocumentAsync(
        Guid claimId,
        string originalFileName,
        string storedFilePath,
        string contentType,
        long fileSizeBytes,
        string documentCategory,
        Guid? documentGroupId,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken);
    Task<InvestigationNoteDto> AddInvestigatorNoteAsync(
        Guid claimId,
        string noteText,
        int? progressPercentSnapshot,
        Guid? createdByUserId,
        CancellationToken cancellationToken);
    Task UpdateInvestigationProgressAsync(Guid claimId, int progressPercent, Guid? changedByUserId, CancellationToken cancellationToken);
    Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken);
    Task AssignInvestigatorAsync(Guid claimId, Guid investigatorUserId, Guid? changedByUserId, CancellationToken cancellationToken);
    Task AssignAdjusterAsync(Guid claimId, Guid adjusterUserId, Guid? changedByUserId, CancellationToken cancellationToken);
    Task SetPriorityAsync(Guid claimId, int priority, Guid? changedByUserId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid claimId, string claimStatus, Guid? changedByUserId, CancellationToken cancellationToken);
    Task UpdateWorkflowStepAsync(Guid claimId, string workflowStep, Guid? changedByUserId, CancellationToken cancellationToken);
}
