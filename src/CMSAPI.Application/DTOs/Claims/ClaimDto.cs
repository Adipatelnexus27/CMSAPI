namespace CMSAPI.Application.DTOs.Claims;

public sealed class ClaimDto
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public long ClaimTypeId { get; set; }
    public long CurrentStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime LossDateUtc { get; set; }
    public DateTime ReportedDateUtc { get; set; }
    public string? IncidentDescription { get; set; }
    public string? LocationOfLoss { get; set; }
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedLossAmount { get; set; }
    public bool IsFraudSuspected { get; set; }
    public ClaimantDetailsDto? Claimant { get; set; }
    public IReadOnlyList<ClaimDocumentDto> Documents { get; set; } = [];
    public IReadOnlyList<RelatedClaimDto> RelatedClaims { get; set; } = [];
}

public sealed class ClaimListItemDto
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public DateTime LossDateUtc { get; set; }
    public DateTime ReportedDateUtc { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
}

public sealed class ClaimantDetailsDto
{
    public string FullName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
}

public sealed class ClaimDocumentDto
{
    public long ClaimDocumentId { get; set; }
    public long DocumentTypeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedDateUtc { get; set; }
    public int VersionNo { get; set; }
}

public sealed class RelatedClaimDto
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
}
