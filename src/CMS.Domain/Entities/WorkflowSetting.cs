namespace CMS.Domain.Entities;

public sealed class WorkflowSetting
{
    public Guid WorkflowSettingId { get; set; }
    public string WorkflowKey { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public int StepSequence { get; set; }
    public string AssignedRole { get; set; } = string.Empty;
    public int SlaHours { get; set; }
    public bool IsActive { get; set; }
}
