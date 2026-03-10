namespace CMS.Domain.Entities;

public sealed class FraudDetectionRule
{
    public Guid FraudRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public int SeverityScore { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}
