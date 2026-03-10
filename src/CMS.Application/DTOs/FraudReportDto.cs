namespace CMS.Application.DTOs;

public sealed class FraudReportDto
{
    public string FraudStatus { get; set; } = string.Empty;
    public int FlagCount { get; set; }
    public int DuplicateFlags { get; set; }
    public int SuspiciousFlags { get; set; }
    public decimal AverageSeverityScore { get; set; }
}
