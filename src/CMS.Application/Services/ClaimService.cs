using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;

namespace CMS.Application.Services;

public sealed class ClaimService : IClaimService
{
    private static readonly HashSet<string> InvestigationDocumentCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Evidence",
        "AccidentPhoto",
        "PoliceReport",
        "MedicalReport"
    };

    private readonly IClaimRepository _claimRepository;
    private readonly IDocumentStorageService _documentStorageService;

    public ClaimService(IClaimRepository claimRepository, IDocumentStorageService documentStorageService)
    {
        _claimRepository = claimRepository;
        _documentStorageService = documentStorageService;
    }

    public async Task<ClaimSummaryDto> RegisterClaimAsync(CreateClaimRequestDto request, CancellationToken cancellationToken)
    {
        ValidateClaimRequest(request);

        var policyIsValid = await _claimRepository.ValidatePolicyAsync(request.PolicyNumber, request.IncidentDateUtc, cancellationToken);
        if (!policyIsValid)
        {
            throw new InvalidOperationException("Policy is invalid or inactive for the incident date.");
        }

        var claimNumber = await _claimRepository.GenerateClaimNumberAsync(cancellationToken);
        var claim = await _claimRepository.CreateClaimAsync(claimNumber, request, cancellationToken);

        foreach (var relatedClaimId in request.RelatedClaimIds.Distinct())
        {
            if (relatedClaimId != claim.ClaimId)
            {
                await _claimRepository.LinkRelatedClaimAsync(claim.ClaimId, relatedClaimId, cancellationToken);
            }
        }

        return claim;
    }

    public async Task<IReadOnlyList<ClaimSummaryDto>> GetClaimsAsync(CancellationToken cancellationToken)
    {
        return await _claimRepository.GetClaimsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimSummaryDto>> GetAssignedClaimsAsync(Guid assigneeUserId, string role, CancellationToken cancellationToken)
    {
        if (assigneeUserId == Guid.Empty)
        {
            throw new InvalidOperationException("Assignee user id is required.");
        }

        if (!string.Equals(role, "Investigator", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(role, "Adjuster", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Role must be Investigator or Adjuster.");
        }

        return await _claimRepository.GetAssignedClaimsAsync(assigneeUserId, role, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimSummaryDto>> GetInvestigationDashboardAsync(CancellationToken cancellationToken)
    {
        return await _claimRepository.GetClaimsAsync(cancellationToken);
    }

    public async Task<ClaimDetailDto> GetClaimDetailAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var documents = await _claimRepository.GetClaimDocumentsAsync(claimId, cancellationToken);
        var related = await _claimRepository.GetRelatedClaimsAsync(claimId, cancellationToken);
        var workflowHistory = await _claimRepository.GetWorkflowHistoryAsync(claimId, cancellationToken);
        var investigationNotes = await _claimRepository.GetInvestigationNotesAsync(claimId, cancellationToken);

        claim.Documents = documents;
        claim.RelatedClaims = related;
        claim.WorkflowHistory = workflowHistory;
        claim.InvestigationNotes = investigationNotes;

        return claim;
    }

    public async Task<ClaimInvestigationDto> GetClaimInvestigationAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var documents = await _claimRepository.GetInvestigationDocumentsAsync(claimId, cancellationToken);
        var notes = await _claimRepository.GetInvestigationNotesAsync(claimId, cancellationToken);

        return new ClaimInvestigationDto
        {
            ClaimId = claim.ClaimId,
            ClaimNumber = claim.ClaimNumber,
            ClaimStatus = claim.ClaimStatus,
            InvestigationProgress = claim.InvestigationProgress,
            Documents = documents,
            Notes = notes
        };
    }

    public async Task<UploadClaimDocumentResponseDto> UploadDocumentAsync(Guid claimId, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, CancellationToken cancellationToken)
    {
        return await UploadDocumentInternalAsync(claimId, "General", originalFileName, contentType, fileSizeBytes, contentStream, cancellationToken);
    }

    public async Task<UploadClaimDocumentResponseDto> UploadInvestigationDocumentAsync(Guid claimId, string documentCategory, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentCategory) || !InvestigationDocumentCategories.Contains(documentCategory.Trim()))
        {
            throw new InvalidOperationException("Document category must be one of: Evidence, AccidentPhoto, PoliceReport, MedicalReport.");
        }

        return await UploadDocumentInternalAsync(claimId, documentCategory.Trim(), originalFileName, contentType, fileSizeBytes, contentStream, cancellationToken);
    }

    public async Task<InvestigationNoteDto> AddInvestigatorNoteAsync(Guid claimId, string noteText, int? progressPercentSnapshot, Guid? createdByUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            throw new InvalidOperationException("Investigator note is required.");
        }

        if (progressPercentSnapshot.HasValue && (progressPercentSnapshot.Value < 0 || progressPercentSnapshot.Value > 100))
        {
            throw new InvalidOperationException("Investigation progress must be between 0 and 100.");
        }

        await EnsureClaimExists(claimId, cancellationToken);

        return await _claimRepository.AddInvestigatorNoteAsync(
            claimId,
            noteText.Trim(),
            progressPercentSnapshot,
            createdByUserId,
            cancellationToken);
    }

    public async Task UpdateInvestigationProgressAsync(Guid claimId, int progressPercent, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (progressPercent < 0 || progressPercent > 100)
        {
            throw new InvalidOperationException("Investigation progress must be between 0 and 100.");
        }

        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.UpdateInvestigationProgressAsync(claimId, progressPercent, changedByUserId, cancellationToken);
    }

    public async Task LinkRelatedClaimAsync(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken)
    {
        if (claimId == relatedClaimId)
        {
            throw new InvalidOperationException("A claim cannot be linked to itself.");
        }

        _ = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Primary claim not found.");

        _ = await _claimRepository.GetClaimByIdAsync(relatedClaimId, cancellationToken)
            ?? throw new InvalidOperationException("Related claim not found.");

        await _claimRepository.LinkRelatedClaimAsync(claimId, relatedClaimId, cancellationToken);
    }

    public async Task AssignInvestigatorAsync(Guid claimId, Guid investigatorUserId, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (investigatorUserId == Guid.Empty) throw new InvalidOperationException("Investigator user id is required.");
        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.AssignInvestigatorAsync(claimId, investigatorUserId, changedByUserId, cancellationToken);
    }

    public async Task AssignAdjusterAsync(Guid claimId, Guid adjusterUserId, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (adjusterUserId == Guid.Empty) throw new InvalidOperationException("Adjuster user id is required.");
        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.AssignAdjusterAsync(claimId, adjusterUserId, changedByUserId, cancellationToken);
    }

    public async Task SetPriorityAsync(Guid claimId, int priority, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (priority < 1 || priority > 5) throw new InvalidOperationException("Priority must be between 1 and 5.");
        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.SetPriorityAsync(claimId, priority, changedByUserId, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid claimId, string claimStatus, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(claimStatus)) throw new InvalidOperationException("Claim status is required.");
        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.UpdateStatusAsync(claimId, claimStatus.Trim(), changedByUserId, cancellationToken);
    }

    public async Task UpdateWorkflowStepAsync(Guid claimId, string workflowStep, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workflowStep)) throw new InvalidOperationException("Workflow step is required.");
        await EnsureClaimExists(claimId, cancellationToken);
        await _claimRepository.UpdateWorkflowStepAsync(claimId, workflowStep.Trim(), changedByUserId, cancellationToken);
    }

    private async Task<UploadClaimDocumentResponseDto> UploadDocumentInternalAsync(
        Guid claimId,
        string documentCategory,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        Stream contentStream,
        CancellationToken cancellationToken)
    {
        ValidateDocumentUpload(originalFileName, fileSizeBytes);

        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken);
        if (claim is null)
        {
            throw new InvalidOperationException("Claim not found.");
        }

        var fullPath = await _documentStorageService.SaveAsync(claim.ClaimNumber, originalFileName, contentStream, cancellationToken);

        var document = await _claimRepository.AddClaimDocumentAsync(
            claimId,
            originalFileName,
            fullPath,
            contentType,
            fileSizeBytes,
            documentCategory,
            cancellationToken);

        return new UploadClaimDocumentResponseDto
        {
            ClaimDocumentId = document.ClaimDocumentId,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            DocumentCategory = document.DocumentCategory,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc
        };
    }

    private async Task EnsureClaimExists(Guid claimId, CancellationToken cancellationToken)
    {
        _ = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");
    }

    private static void ValidateClaimRequest(CreateClaimRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PolicyNumber)) throw new InvalidOperationException("Policy number is required.");
        if (string.IsNullOrWhiteSpace(request.ClaimType)) throw new InvalidOperationException("Claim type is required.");
        if (string.IsNullOrWhiteSpace(request.ReporterName)) throw new InvalidOperationException("Reporter name is required.");
        if (request.IncidentDateUtc == default) throw new InvalidOperationException("Incident date is required.");
        if (request.IncidentDateUtc > DateTime.UtcNow.AddMinutes(5)) throw new InvalidOperationException("Incident date cannot be in the future.");
        if (string.IsNullOrWhiteSpace(request.IncidentLocation)) throw new InvalidOperationException("Incident location is required.");
        if (string.IsNullOrWhiteSpace(request.IncidentDescription)) throw new InvalidOperationException("Incident description is required.");
    }

    private static void ValidateDocumentUpload(string originalFileName, long fileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("Document name is required.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new InvalidOperationException("Document is empty.");
        }

        if (fileSizeBytes > 25 * 1024 * 1024)
        {
            throw new InvalidOperationException("Document exceeds 25 MB limit.");
        }
    }
}
