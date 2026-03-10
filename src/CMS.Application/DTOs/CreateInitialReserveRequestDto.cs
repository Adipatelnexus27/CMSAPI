namespace CMS.Application.DTOs;

public sealed class CreateInitialReserveRequestDto
{
    public decimal ReserveAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Reason { get; set; }
}
