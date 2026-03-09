namespace CMSAPI.Domain.Entities;

public sealed class WorkflowStageMaster
{
    public long WorkflowStageId { get; set; }
    public long WorkflowDefinitionId { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public int StageSequence { get; set; }
    public int? SlaInHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

