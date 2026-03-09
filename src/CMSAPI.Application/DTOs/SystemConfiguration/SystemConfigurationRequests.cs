namespace CMSAPI.Application.DTOs.SystemConfiguration;

public sealed class UpsertClaimTypeRequest
{
    public string ClaimTypeCode { get; set; } = string.Empty;
    public string ClaimTypeName { get; set; } = string.Empty;
    public string? ClaimTypeDescription { get; set; }
}

public sealed class UpsertClaimStatusRequest
{
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    public bool IsTerminalStatus { get; set; }
}

public sealed class UpsertInsuranceProductRequest
{
    public string PolicyTypeCode { get; set; } = string.Empty;
    public string PolicyTypeName { get; set; } = string.Empty;
    public string? PolicyTypeDescription { get; set; }
}

public sealed class UpsertFraudRuleRequest
{
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public decimal RuleWeight { get; set; }
    public string? RuleDefinition { get; set; }
}

public sealed class UpsertWorkflowStageRequest
{
    public long WorkflowDefinitionId { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public int StageSequence { get; set; }
    public int? SlaInHours { get; set; }
}

