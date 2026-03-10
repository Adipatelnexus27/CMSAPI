namespace CMS.Application.DTOs;

public sealed class ClaimReserveDto
{
    public Guid ClaimReserveId { get; set; }
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal CurrentReserveAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? LastApprovedAtUtc { get; set; }
    public Guid? LastApprovedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
