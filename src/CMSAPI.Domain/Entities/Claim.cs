using CMSAPI.Domain.Enums;

namespace CMSAPI.Domain.Entities;

public sealed class Claim : BaseEntity
{
    public required string ClaimNumber { get; set; }
    public required string PolicyNumber { get; set; }
    public required string ClaimantName { get; set; }
    public DateTime IncidentDateUtc { get; set; }
    public decimal ClaimedAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Registered;
    public string? Description { get; set; }
}

