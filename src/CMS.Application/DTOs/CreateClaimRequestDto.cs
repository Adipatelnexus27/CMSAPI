namespace CMS.Application.DTOs;

public sealed class CreateClaimRequestDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public string IncidentLocation { get; set; } = string.Empty;
    public string IncidentDescription { get; set; } = string.Empty;
    public IReadOnlyList<Guid> RelatedClaimIds { get; set; } = [];
}
