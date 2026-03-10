namespace CMS.Application.DTOs;

public sealed class ClaimPaymentDto
{
    public Guid ClaimPaymentId { get; set; }
    public Guid ClaimSettlementId { get; set; }
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string PaymentStatus { get; set; } = string.Empty;
    public string? RequestNote { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovalNote { get; set; }
    public string? StatusNote { get; set; }
    public DateTime LastStatusUpdatedAtUtc { get; set; }
}
