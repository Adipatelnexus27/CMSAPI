namespace CMSAPI.Application.DTOs.Claims;

public sealed class CreateClaimRequestDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public long ClaimTypeId { get; set; }
    public DateTime LossDateUtc { get; set; }
    public string? IncidentDescription { get; set; }
    public string? LocationOfLoss { get; set; }
    public decimal EstimatedLossAmount { get; set; }

    public string ClaimantName { get; set; } = string.Empty;
    public string? ClaimantContactNo { get; set; }
    public string? ClaimantEmail { get; set; }
    public string? ClaimantAddressLine { get; set; }
    public string? ClaimantCity { get; set; }
    public string? ClaimantState { get; set; }
    public string? ClaimantPostalCode { get; set; }

    public IReadOnlyCollection<long> RelatedClaimIds { get; set; } = [];
}
