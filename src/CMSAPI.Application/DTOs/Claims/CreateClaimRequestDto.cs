namespace CMSAPI.Application.DTOs.Claims;

public sealed class CreateClaimRequestDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public decimal ClaimedAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public string? Description { get; set; }
}

