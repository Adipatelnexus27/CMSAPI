namespace CMSAPI.Application.DTOs.SystemConfiguration;

public sealed class ClaimTypeDto
{
    public long ClaimTypeId { get; set; }
    public string ClaimTypeCode { get; set; } = string.Empty;
    public string ClaimTypeName { get; set; } = string.Empty;
    public string? ClaimTypeDescription { get; set; }
}

public sealed class ClaimStatusDto
{
    public long ClaimStatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    public bool IsTerminalStatus { get; set; }
}

public sealed class InsuranceProductDto
{
    public long PolicyTypeId { get; set; }
    public string PolicyTypeCode { get; set; } = string.Empty;
    public string PolicyTypeName { get; set; } = string.Empty;
    public string? PolicyTypeDescription { get; set; }
}

public sealed class FraudRuleDto
{
    public long FraudRuleId { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public decimal RuleWeight { get; set; }
    public string? RuleDefinition { get; set; }
}

public sealed class WorkflowStageDto
{
    public long WorkflowStageId { get; set; }
    public long WorkflowDefinitionId { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public int StageSequence { get; set; }
    public int? SlaInHours { get; set; }
}

