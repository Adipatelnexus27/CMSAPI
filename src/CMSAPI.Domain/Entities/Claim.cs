namespace CMSAPI.Domain.Entities;

public sealed class Claim
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public long PolicyId { get; set; }
    public long ClaimTypeId { get; set; }
    public long CurrentStatusId { get; set; }
    public DateTime LossDate { get; set; }
    public DateTime ReportedDate { get; set; }
    public string? IncidentDescription { get; set; }
    public string? LocationOfLoss { get; set; }
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedLossAmount { get; set; }
    public long CurrencyId { get; set; }
    public decimal? FraudScore { get; set; }
    public bool IsFraudSuspected { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
