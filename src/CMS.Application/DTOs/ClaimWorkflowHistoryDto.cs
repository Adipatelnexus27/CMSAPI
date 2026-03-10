namespace CMS.Application.DTOs;

public sealed class ClaimWorkflowHistoryDto
{
    public Guid ClaimWorkflowHistoryId { get; set; }
    public Guid ClaimId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
