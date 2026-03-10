namespace CMS.Application.DTOs;

public sealed class AdjustReserveRequestDto
{
    public decimal ReserveAmount { get; set; }
    public string? Reason { get; set; }
}
