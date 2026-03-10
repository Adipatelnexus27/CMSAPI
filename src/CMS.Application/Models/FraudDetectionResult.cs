namespace CMS.Application.Models;

public sealed class FraudDetectionResult
{
    public string FlagType { get; set; } = string.Empty;
    public string? RuleName { get; set; }
    public int SeverityScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public bool IsSuspicious { get; set; }
}
