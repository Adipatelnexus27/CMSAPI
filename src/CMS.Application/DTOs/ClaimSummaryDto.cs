namespace CMS.Application.DTOs;

public sealed class ClaimSummaryDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
