namespace CMS.Application.DTOs;

public sealed class ClaimReserveHistoryDto
{
    public Guid ClaimReserveHistoryId { get; set; }
    public Guid ClaimReserveId { get; set; }
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public decimal? PreviousReserveAmount { get; set; }
    public decimal RequestedReserveAmount { get; set; }
    public decimal? ApprovedReserveAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovalNote { get; set; }
}
