using System.Security.Claims;
using CMSAPI.Application.DTOs.ClaimAssignment;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Application.Security;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMSAPI.API.Controllers;

[ApiController]
[Route("api/claim-assignments")]
[Authorize]
public sealed class ClaimAssignmentsController : ControllerBase
{
    private readonly IClaimAssignmentService _assignmentService;

    public ClaimAssignmentsController(IClaimAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet("dashboard")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAssign)]
    [ProducesResponseType(typeof(ClaimAssignmentDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var data = await _assignmentService.GetDashboardAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost("{claimId:long}/triage")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAssign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TriageClaim(long claimId, [FromBody] TriageClaimRequestDto request, CancellationToken cancellationToken)
    {
        await _assignmentService.TriageClaimAsync(claimId, request, GetCurrentUserId(), GetCurrentUserName(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{claimId:long}/assign-investigator")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAssign)]
    [ProducesResponseType(typeof(AssignmentResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignInvestigator(long claimId, [FromBody] AssignClaimRoleRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.AssignInvestigatorAsync(
            claimId,
            request,
            GetCurrentUserId(),
            GetCurrentUserName(),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{claimId:long}/assign-adjuster")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAssign)]
    [ProducesResponseType(typeof(AssignmentResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignAdjuster(long claimId, [FromBody] AssignClaimRoleRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.AssignAdjusterAsync(
            claimId,
            request,
            GetCurrentUserId(),
            GetCurrentUserName(),
            cancellationToken);
        return Ok(result);
    }

    private long GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(CustomClaimTypes.UserId);

        return long.TryParse(rawUserId, out var userId) ? userId : 0;
    }

    private string GetCurrentUserName() =>
        User.FindFirstValue(ClaimTypes.Name) ?? "api";
}
