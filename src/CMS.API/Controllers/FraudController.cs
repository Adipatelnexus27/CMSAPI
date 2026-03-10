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
public sealed class FraudController : ControllerBase
{
    private readonly IFraudService _fraudService;

    public FraudController(IFraudService fraudService)
    {
        _fraudService = fraudService;
    }

    [HttpPost("detect/{claimId:guid}")]
    [RequirePermission("Fraud.Detect")]
    public async Task<IActionResult> RunDetection(Guid claimId, CancellationToken cancellationToken)
    {
        var response = await _fraudService.RunDetectionAsync(claimId, GetCurrentUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("flags")]
    [RequirePermission("Fraud.Flags.Read")]
    public async Task<IActionResult> GetFlags([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var flags = await _fraudService.GetFraudFlagsAsync(status, cancellationToken);
        return Ok(flags);
    }

    [HttpGet("claims/{claimId:guid}/flags")]
    [RequirePermission("Fraud.Flags.Read")]
    public async Task<IActionResult> GetClaimFlags(Guid claimId, CancellationToken cancellationToken)
    {
        var flags = await _fraudService.GetClaimFraudFlagsAsync(claimId, cancellationToken);
        return Ok(flags);
    }

    [HttpPut("flags/{fraudFlagId:guid}/status")]
    [RequirePermission("Fraud.Flags.Update")]
    public async Task<IActionResult> UpdateFlagStatus(Guid fraudFlagId, [FromBody] UpdateFraudFlagStatusRequestDto request, CancellationToken cancellationToken)
    {
        await _fraudService.UpdateFraudFlagStatusAsync(fraudFlagId, request.Status, request.ReviewNote, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
