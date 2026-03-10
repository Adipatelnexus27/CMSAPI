using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;

namespace CMS.Application.Services;

public sealed class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly string _storageRoot;

    public ClaimService(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
        _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads", "claims");
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

    public async Task<ClaimDetailDto> GetClaimDetailAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var documents = await _claimRepository.GetClaimDocumentsAsync(claimId, cancellationToken);
        var related = await _claimRepository.GetRelatedClaimsAsync(claimId, cancellationToken);
        var workflowHistory = await _claimRepository.GetWorkflowHistoryAsync(claimId, cancellationToken);

        claim.Documents = documents;
        claim.RelatedClaims = related;
        claim.WorkflowHistory = workflowHistory;

        return claim;
    }

    public async Task<UploadClaimDocumentResponseDto> UploadDocumentAsync(Guid claimId, string originalFileName, string contentType, long fileSizeBytes, Stream contentStream, CancellationToken cancellationToken)
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

        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken);
        if (claim is null)
        {
            throw new InvalidOperationException("Claim not found.");
        }

        Directory.CreateDirectory(_storageRoot);

        var safeFileName = SanitizeFileName(originalFileName);
        var extension = Path.GetExtension(safeFileName);
        var generatedName = $"{claim.ClaimNumber}_{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storageRoot, generatedName);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        var document = await _claimRepository.AddClaimDocumentAsync(
            claimId,
            originalFileName,
            fullPath,
            contentType,
            fileSizeBytes,
            cancellationToken);

        return new UploadClaimDocumentResponseDto
        {
            ClaimDocumentId = document.ClaimDocumentId,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc
        };
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

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "document.bin" : sanitized;
    }
}
