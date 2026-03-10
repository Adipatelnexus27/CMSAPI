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
public sealed class ClaimReservesController : ControllerBase
{
    private readonly IClaimReserveService _claimReserveService;

    public ClaimReservesController(IClaimReserveService claimReserveService)
    {
        _claimReserveService = claimReserveService;
    }

    [HttpPost("claims/{claimId:guid}/initial")]
    [RequirePermission("Claims.Reserve.Initial")]
    public async Task<IActionResult> CreateInitialReserve(Guid claimId, [FromBody] CreateInitialReserveRequestDto request, CancellationToken cancellationToken)
    {
        var reserve = await _claimReserveService.CreateInitialReserveAsync(claimId, request, GetCurrentUserId(), cancellationToken);
        return Ok(reserve);
    }

    [HttpPost("claims/{claimId:guid}/adjust")]
    [RequirePermission("Claims.Reserve.Adjust")]
    public async Task<IActionResult> AdjustReserve(Guid claimId, [FromBody] AdjustReserveRequestDto request, CancellationToken cancellationToken)
    {
        var history = await _claimReserveService.AdjustReserveAsync(claimId, request, GetCurrentUserId(), cancellationToken);
        return Ok(history);
    }

    [HttpGet("claims/{claimId:guid}")]
    [RequirePermission("Claims.Reserve.Read")]
    public async Task<IActionResult> GetClaimReserve(Guid claimId, CancellationToken cancellationToken)
    {
        var reserve = await _claimReserveService.GetClaimReserveAsync(claimId, cancellationToken);
        return Ok(reserve);
    }

    [HttpGet("claims/{claimId:guid}/history")]
    [RequirePermission("Claims.Reserve.Read")]
    public async Task<IActionResult> GetReserveHistory(Guid claimId, CancellationToken cancellationToken)
    {
        var history = await _claimReserveService.GetReserveHistoryAsync(claimId, cancellationToken);
        return Ok(history);
    }

    [HttpGet("approvals")]
    [RequirePermission("Claims.Reserve.Approve")]
    public async Task<IActionResult> GetApprovalQueue([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var history = await _claimReserveService.GetReserveApprovalQueueAsync(status, cancellationToken);
        return Ok(history);
    }

    [HttpPut("history/{claimReserveHistoryId:guid}/approve")]
    [RequirePermission("Claims.Reserve.Approve")]
    public async Task<IActionResult> ApproveReserveAdjustment(Guid claimReserveHistoryId, [FromBody] ReviewReserveRequestDto request, CancellationToken cancellationToken)
    {
        await _claimReserveService.ApproveReserveAdjustmentAsync(claimReserveHistoryId, request.ApprovalNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("history/{claimReserveHistoryId:guid}/reject")]
    [RequirePermission("Claims.Reserve.Approve")]
    public async Task<IActionResult> RejectReserveAdjustment(Guid claimReserveHistoryId, [FromBody] ReviewReserveRequestDto request, CancellationToken cancellationToken)
    {
        await _claimReserveService.RejectReserveAdjustmentAsync(claimReserveHistoryId, request.ApprovalNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
