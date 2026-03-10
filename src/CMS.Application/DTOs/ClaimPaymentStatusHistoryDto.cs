namespace CMS.Application.DTOs;

public sealed class ClaimPaymentStatusHistoryDto
{
    public Guid ClaimPaymentStatusHistoryId { get; set; }
    public Guid ClaimPaymentId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
