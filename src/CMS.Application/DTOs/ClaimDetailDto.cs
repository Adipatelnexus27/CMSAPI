namespace CMS.Application.DTOs;

public sealed class ClaimDetailDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public string IncidentLocation { get; set; } = string.Empty;
    public string IncidentDescription { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<ClaimDocumentDto> Documents { get; set; } = [];
    public IReadOnlyList<RelatedClaimDto> RelatedClaims { get; set; } = [];
}
