namespace CMS.Application.DTOs;

public sealed class UpsertFraudDetectionRuleRequestDto
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public int SeverityScore { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}
