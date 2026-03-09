using System.Security.Cryptography;
using CMSAPI.Application.BusinessRules;
using CMSAPI.Application.DTOs.Claims;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Interfaces;
using FluentValidation;

namespace CMSAPI.Application.Services;

public sealed class ClaimService : IClaimService
{
    private const string ClaimantPartyType = "Claimant";
    private const string RelatedClaimCategory = "RelatedClaim";

    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClaimBusinessRules _businessRules;
    private readonly IFileStorageService _fileStorageService;
    private readonly IValidator<CreateClaimRequestDto> _createValidator;
    private readonly IValidator<UpdateClaimStatusRequestDto> _statusValidator;
    private readonly IValidator<UploadClaimDocumentRequestDto> _uploadDocumentValidator;

    public ClaimService(
        IClaimRepository claimRepository,
        IPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        ClaimBusinessRules businessRules,
        IFileStorageService fileStorageService,
        IValidator<CreateClaimRequestDto> createValidator,
        IValidator<UpdateClaimStatusRequestDto> statusValidator,
        IValidator<UploadClaimDocumentRequestDto> uploadDocumentValidator)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _businessRules = businessRules;
        _fileStorageService = fileStorageService;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
        _uploadDocumentValidator = uploadDocumentValidator;
    }

    public async Task<IReadOnlyList<ClaimListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _claimRepository.GetAllAsync(cancellationToken);
        var policyNumbers = await _policyRepository.GetPolicyNumbersByIdsAsync(claims.Select(x => x.PolicyId), cancellationToken);
        var claimants = await _claimRepository.GetClaimantPartiesByClaimIdsAsync(claims.Select(x => x.ClaimId), cancellationToken);
        var claimantLookup = claimants
            .GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(party => party.CreatedDate).First().FullName);

        return claims.Select(claim => new ClaimListItemDto
        {
            ClaimId = claim.ClaimId,
            ClaimNumber = claim.ClaimNumber,
            PolicyNumber = policyNumbers.GetValueOrDefault(claim.PolicyId, string.Empty),
            ClaimantName = claimantLookup.GetValueOrDefault(claim.ClaimId, "Unknown"),
            LossDateUtc = claim.LossDate,
            ReportedDateUtc = claim.ReportedDate,
            StatusName = GetStatusName(claim.CurrentStatusId),
            EstimatedLossAmount = claim.EstimatedLossAmount
        }).ToList();
    }

    public async Task<ClaimDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdAsync(id, cancellationToken);
        if (claim is null)
        {
            return null;
        }

        var policy = await _policyRepository.GetByIdAsync(claim.PolicyId, cancellationToken);
        var claimant = await _claimRepository.GetClaimantPartyAsync(id, cancellationToken);
        var documents = await _claimRepository.GetDocumentsByClaimIdAsync(id, cancellationToken);
        var relatedClaims = await ResolveRelatedClaimsAsync(id, cancellationToken);

        return ToClaimDto(claim, policy?.PolicyNumber ?? string.Empty, claimant, documents, relatedClaims);
    }

    public async Task<ClaimDto> CreateAsync(
        CreateClaimRequestDto request,
        long createdByUserId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var normalizedPolicyNumber = request.PolicyNumber.Trim();
        var policy = await _businessRules.EnsurePolicyIsEligibleForClaimAsync(normalizedPolicyNumber, request.LossDateUtc, cancellationToken);

        var claimNumber = await GenerateClaimNumberAsync(cancellationToken);

        var entity = new Claim
        {
            ClaimNumber = claimNumber,
            PolicyId = policy.PolicyId,
            ClaimTypeId = request.ClaimTypeId,
            CurrentStatusId = (long)ClaimStatus.Registered,
            LossDate = request.LossDateUtc,
            ReportedDate = DateTime.UtcNow,
            IncidentDescription = request.IncidentDescription?.Trim(),
            LocationOfLoss = request.LocationOfLoss?.Trim(),
            EstimatedLossAmount = request.EstimatedLossAmount,
            CurrencyId = policy.CurrencyId,
            IsFraudSuspected = false,
            CreatedBy = createdBy
        };

        var claimant = new ClaimParty
        {
            PartyType = ClaimantPartyType,
            FullName = request.ClaimantName.Trim(),
            ContactNo = request.ClaimantContactNo?.Trim(),
            Email = request.ClaimantEmail?.Trim(),
            AddressLine = request.ClaimantAddressLine?.Trim(),
            City = request.ClaimantCity?.Trim(),
            State = request.ClaimantState?.Trim(),
            PostalCode = request.ClaimantPostalCode?.Trim(),
            CreatedBy = createdBy
        };

        await _claimRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        claimant.ClaimId = entity.ClaimId;
        await _claimRepository.AddPartyAsync(claimant, cancellationToken);

        foreach (var relatedClaimId in request.RelatedClaimIds.Distinct())
        {
            await AddRelatedClaimLinkIfNotExistsAsync(entity.ClaimId, relatedClaimId, createdByUserId, createdBy, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var relatedClaims = await ResolveRelatedClaimsAsync(entity.ClaimId, cancellationToken);
        return ToClaimDto(entity, policy.PolicyNumber, claimant, [], relatedClaims);
    }

    public async Task<ClaimDto> UpdateStatusAsync(
        long id,
        UpdateClaimStatusRequestDto request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        await _statusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var claim = await _claimRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim '{id}' was not found.");

        var currentStatus = ToClaimStatus(claim.CurrentStatusId);
        _businessRules.EnsureStatusTransitionAllowed(currentStatus, request.Status);

        claim.CurrentStatusId = (long)request.Status;
        claim.ModifiedDate = DateTime.UtcNow;
        claim.ModifiedBy = modifiedBy;

        _claimRepository.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var policy = await _policyRepository.GetByIdAsync(claim.PolicyId, cancellationToken);
        var claimant = await _claimRepository.GetClaimantPartyAsync(claim.ClaimId, cancellationToken);
        var documents = await _claimRepository.GetDocumentsByClaimIdAsync(claim.ClaimId, cancellationToken);
        var relatedClaims = await ResolveRelatedClaimsAsync(claim.ClaimId, cancellationToken);

        return ToClaimDto(claim, policy?.PolicyNumber ?? string.Empty, claimant, documents, relatedClaims);
    }

    public async Task<ClaimDocumentDto> UploadDocumentAsync(
        long claimId,
        UploadClaimDocumentRequestDto request,
        long uploadedByUserId,
        string uploadedBy,
        CancellationToken cancellationToken = default)
    {
        await _uploadDocumentValidator.ValidateAndThrowAsync(request, cancellationToken);

        var claimExists = await _claimRepository.ExistsByIdAsync(claimId, cancellationToken);
        if (!claimExists)
        {
            throw new KeyNotFoundException($"Claim '{claimId}' was not found.");
        }

        var filePath = await _fileStorageService.SaveFileAsync(request.FileName, request.Content, cancellationToken);
        var versionNo = await _claimRepository.GetMaxDocumentVersionAsync(claimId, request.FileName, cancellationToken) + 1;

        var document = new ClaimDocument
        {
            ClaimId = claimId,
            DocumentTypeId = request.DocumentTypeId,
            FileName = request.FileName,
            FilePath = filePath,
            FileHash = Convert.ToHexString(SHA256.HashData(request.Content)),
            UploadedDate = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId,
            VersionNo = versionNo,
            CreatedBy = uploadedBy
        };

        await _claimRepository.AddDocumentAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToClaimDocumentDto(document);
    }

    public async Task LinkRelatedClaimAsync(long claimId, long relatedClaimId, long userId, string userName, CancellationToken cancellationToken = default)
    {
        await AddRelatedClaimLinkIfNotExistsAsync(claimId, relatedClaimId, userId, userName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateClaimNumberAsync(CancellationToken cancellationToken)
    {
        for (var retry = 0; retry < 5; retry++)
        {
            var claimNumber = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            if (!await _claimRepository.ExistsByClaimNumberAsync(claimNumber, cancellationToken))
            {
                return claimNumber;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique claim number. Please retry.");
    }

    private async Task AddRelatedClaimLinkIfNotExistsAsync(
        long claimId,
        long relatedClaimId,
        long userId,
        string userName,
        CancellationToken cancellationToken)
    {
        if (claimId == relatedClaimId)
        {
            throw new InvalidOperationException("A claim cannot be linked to itself.");
        }

        if (!await _claimRepository.ExistsByIdAsync(claimId, cancellationToken))
        {
            throw new KeyNotFoundException($"Claim '{claimId}' was not found.");
        }

        if (!await _claimRepository.ExistsByIdAsync(relatedClaimId, cancellationToken))
        {
            throw new KeyNotFoundException($"Related claim '{relatedClaimId}' was not found.");
        }

        if (await _claimRepository.RelatedClaimLinkExistsAsync(claimId, relatedClaimId, cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        await _claimRepository.AddNoteAsync(new ClaimNote
        {
            ClaimId = claimId,
            NoteCategory = RelatedClaimCategory,
            NoteText = $"RelatedClaimId:{relatedClaimId}",
            NotedDate = now,
            NotedByUserId = userId,
            CreatedBy = userName
        }, cancellationToken);

        await _claimRepository.AddNoteAsync(new ClaimNote
        {
            ClaimId = relatedClaimId,
            NoteCategory = RelatedClaimCategory,
            NoteText = $"RelatedClaimId:{claimId}",
            NotedDate = now,
            NotedByUserId = userId,
            CreatedBy = userName
        }, cancellationToken);
    }

    private async Task<IReadOnlyList<RelatedClaimDto>> ResolveRelatedClaimsAsync(long claimId, CancellationToken cancellationToken)
    {
        var notes = await _claimRepository.GetNotesByClaimIdAsync(claimId, RelatedClaimCategory, cancellationToken);
        var relatedIds = notes
            .Select(ParseRelatedClaimId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (relatedIds.Count == 0)
        {
            return [];
        }

        var relatedClaims = await _claimRepository.GetByIdsAsync(relatedIds, cancellationToken);
        return relatedClaims
            .Select(x => new RelatedClaimDto
            {
                ClaimId = x.ClaimId,
                ClaimNumber = x.ClaimNumber
            })
            .OrderBy(x => x.ClaimId)
            .ToList();
    }

    private static long? ParseRelatedClaimId(ClaimNote note)
    {
        const string prefix = "RelatedClaimId:";
        if (!note.NoteText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return long.TryParse(note.NoteText[prefix.Length..], out var parsed) ? parsed : null;
    }

    private static ClaimDto ToClaimDto(
        Claim claim,
        string policyNumber,
        ClaimParty? claimant,
        IReadOnlyList<ClaimDocument> documents,
        IReadOnlyList<RelatedClaimDto> relatedClaims)
    {
        return new ClaimDto
        {
            ClaimId = claim.ClaimId,
            ClaimNumber = claim.ClaimNumber,
            PolicyNumber = policyNumber,
            ClaimTypeId = claim.ClaimTypeId,
            CurrentStatusId = claim.CurrentStatusId,
            StatusName = GetStatusName(claim.CurrentStatusId),
            LossDateUtc = claim.LossDate,
            ReportedDateUtc = claim.ReportedDate,
            IncidentDescription = claim.IncidentDescription,
            LocationOfLoss = claim.LocationOfLoss,
            EstimatedLossAmount = claim.EstimatedLossAmount,
            ApprovedLossAmount = claim.ApprovedLossAmount,
            IsFraudSuspected = claim.IsFraudSuspected,
            Claimant = claimant is null
                ? null
                : new ClaimantDetailsDto
                {
                    FullName = claimant.FullName,
                    ContactNo = claimant.ContactNo,
                    Email = claimant.Email,
                    AddressLine = claimant.AddressLine,
                    City = claimant.City,
                    State = claimant.State,
                    PostalCode = claimant.PostalCode
                },
            Documents = documents.Select(ToClaimDocumentDto).ToList(),
            RelatedClaims = relatedClaims
        };
    }

    private static ClaimDocumentDto ToClaimDocumentDto(ClaimDocument document) => new()
    {
        ClaimDocumentId = document.ClaimDocumentId,
        DocumentTypeId = document.DocumentTypeId,
        FileName = document.FileName,
        FilePath = document.FilePath,
        UploadedDateUtc = document.UploadedDate,
        VersionNo = document.VersionNo
    };

    private static ClaimStatus ToClaimStatus(long statusId)
    {
        if (!Enum.IsDefined(typeof(ClaimStatus), (int)statusId))
        {
            throw new InvalidOperationException($"Unknown claim status id '{statusId}'.");
        }

        return (ClaimStatus)(int)statusId;
    }

    private static string GetStatusName(long statusId)
    {
        return Enum.IsDefined(typeof(ClaimStatus), (int)statusId)
            ? ((ClaimStatus)(int)statusId).ToString()
            : $"Status-{statusId}";
    }
}
