namespace CMS.Application.DTOs;

public sealed class RequestPaymentApprovalDto
{
    public decimal PaymentAmount { get; set; }
    public string? RequestNote { get; set; }
}
