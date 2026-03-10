using System.Security.Claims;
using CMS.API.Middlewares;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClaimSettlementsController : ControllerBase
{
    private readonly IClaimSettlementService _claimSettlementService;

    public ClaimSettlementsController(IClaimSettlementService claimSettlementService)
    {
        _claimSettlementService = claimSettlementService;
    }

    [HttpPost("claims/{claimId:guid}/calculate")]
    [RequirePermission("Claims.Settlement.Calculate")]
    public async Task<IActionResult> CalculateSettlement(Guid claimId, [FromBody] CalculateSettlementRequestDto request, CancellationToken cancellationToken)
    {
        var settlement = await _claimSettlementService.CalculateSettlementAsync(claimId, request, GetCurrentUserId(), cancellationToken);
        return Ok(settlement);
    }

    [HttpGet("claims/{claimId:guid}")]
    [RequirePermission("Claims.Settlement.Read")]
    public async Task<IActionResult> GetSettlementByClaimId(Guid claimId, CancellationToken cancellationToken)
    {
        var settlement = await _claimSettlementService.GetSettlementByClaimIdAsync(claimId, cancellationToken);
        return Ok(settlement);
    }

    [HttpGet("claims/{claimId:guid}/payments")]
    [RequirePermission("Payments.Read")]
    public async Task<IActionResult> GetClaimPayments(Guid claimId, CancellationToken cancellationToken)
    {
        var payments = await _claimSettlementService.GetPaymentsByClaimIdAsync(claimId, cancellationToken);
        return Ok(payments);
    }

    [HttpPost("{claimSettlementId:guid}/payments/request")]
    [RequirePermission("Payments.Request")]
    public async Task<IActionResult> RequestPaymentApproval(Guid claimSettlementId, [FromBody] RequestPaymentApprovalDto request, CancellationToken cancellationToken)
    {
        var payment = await _claimSettlementService.RequestPaymentApprovalAsync(claimSettlementId, request, GetCurrentUserId(), cancellationToken);
        return Ok(payment);
    }

    [HttpGet("payments")]
    [RequirePermission("Payments.Read")]
    public async Task<IActionResult> GetPayments([FromQuery] string? paymentStatus, CancellationToken cancellationToken)
    {
        var payments = await _claimSettlementService.GetPaymentsAsync(paymentStatus, cancellationToken);
        return Ok(payments);
    }

    [HttpGet("payments/{claimPaymentId:guid}/history")]
    [RequirePermission("Payments.Read")]
    public async Task<IActionResult> GetPaymentStatusHistory(Guid claimPaymentId, CancellationToken cancellationToken)
    {
        var history = await _claimSettlementService.GetPaymentStatusHistoryAsync(claimPaymentId, cancellationToken);
        return Ok(history);
    }

    [HttpPut("payments/{claimPaymentId:guid}/approve")]
    [RequirePermission("Payments.Approve")]
    public async Task<IActionResult> ApprovePayment(Guid claimPaymentId, [FromBody] ReviewPaymentRequestDto request, CancellationToken cancellationToken)
    {
        await _claimSettlementService.ApprovePaymentAsync(claimPaymentId, request.ApprovalNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("payments/{claimPaymentId:guid}/reject")]
    [RequirePermission("Payments.Approve")]
    public async Task<IActionResult> RejectPayment(Guid claimPaymentId, [FromBody] ReviewPaymentRequestDto request, CancellationToken cancellationToken)
    {
        await _claimSettlementService.RejectPaymentAsync(claimPaymentId, request.ApprovalNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("payments/{claimPaymentId:guid}/status")]
    [RequirePermission("Payments.Status.Update")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid claimPaymentId, [FromBody] UpdatePaymentStatusRequestDto request, CancellationToken cancellationToken)
    {
        await _claimSettlementService.UpdatePaymentStatusAsync(claimPaymentId, request.PaymentStatus, request.StatusNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
