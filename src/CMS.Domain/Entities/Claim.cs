namespace CMS.Domain.Entities;

public sealed class Claim
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
}
