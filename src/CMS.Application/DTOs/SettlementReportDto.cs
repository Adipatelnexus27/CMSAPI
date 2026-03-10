namespace CMS.Application.DTOs;

public sealed class SettlementReportDto
{
    public string PaymentStatus { get; set; } = string.Empty;
    public int PaymentCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public decimal AveragePaymentAmount { get; set; }
}
