using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.DTOs.Claims;

public sealed class ClaimDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public decimal ClaimedAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public ClaimStatus Status { get; set; }
    public string? Description { get; set; }
}

