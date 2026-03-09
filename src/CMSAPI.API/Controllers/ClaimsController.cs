using CMSAPI.Application.DTOs.Claims;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Application.Security;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMSAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    [HttpGet]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)},{nameof(UserRole.FraudAnalyst)}",
        Policy = PermissionPolicies.ClaimsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetAllAsync(cancellationToken);
        return Ok(claims);
    }

    [HttpGet("{id:guid}")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)},{nameof(UserRole.FraudAnalyst)}",
        Policy = PermissionPolicies.ClaimsRead)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetByIdAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpPost]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)}",
        Policy = PermissionPolicies.ClaimsCreate)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _claimService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAdjudicate)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateClaimStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _claimService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }
}
