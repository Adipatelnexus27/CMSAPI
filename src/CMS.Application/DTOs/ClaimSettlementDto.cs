namespace CMS.Application.DTOs;

public sealed class ClaimSettlementDto
{
    public Guid ClaimSettlementId { get; set; }
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal GrossLossAmount { get; set; }
    public decimal PolicyLimitAmount { get; set; }
    public decimal DeductibleAmount { get; set; }
    public decimal EligibleAmount { get; set; }
    public decimal ApprovedSettlementAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string CalculationStatus { get; set; } = string.Empty;
    public Guid? CalculatedByUserId { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
