namespace CMS.Application.DTOs;

public sealed class UpdatePaymentStatusRequestDto
{
    public string PaymentStatus { get; set; } = string.Empty;
    public string? StatusNote { get; set; }
}
