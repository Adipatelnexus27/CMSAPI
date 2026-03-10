namespace CMS.Application.DTOs;

public sealed class CalculateSettlementRequestDto
{
    public decimal GrossLossAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}
