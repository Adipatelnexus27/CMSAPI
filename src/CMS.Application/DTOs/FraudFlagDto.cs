namespace CMS.Application.DTOs;

public sealed class FraudFlagDto
{
    public Guid FraudFlagId { get; set; }
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string FlagType { get; set; } = string.Empty;
    public string? RuleName { get; set; }
    public int SeverityScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public bool IsSuspicious { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewNote { get; set; }
}
