using CMSAPI.Application.DTOs.Claims;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Application.Security;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [ProducesResponseType(typeof(IReadOnlyList<ClaimListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetAllAsync(cancellationToken);
        return Ok(claims);
    }

    [HttpGet("{id:long}")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)},{nameof(UserRole.FraudAnalyst)}",
        Policy = PermissionPolicies.ClaimsRead)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetByIdAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpGet("{id:long}/transitions")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)},{nameof(UserRole.FraudAnalyst)}",
        Policy = PermissionPolicies.ClaimsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimWorkflowTransitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllowedTransitions(long id, CancellationToken cancellationToken)
    {
        var transitions = await _claimService.GetAllowedTransitionsAsync(id, cancellationToken);
        return Ok(transitions);
    }

    [HttpPost]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)}",
        Policy = PermissionPolicies.ClaimsCreate)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var created = await _claimService.CreateAsync(request, userId, userName, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.ClaimId }, created);
    }

    [HttpPatch("{id:long}/status")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}",
        Policy = PermissionPolicies.ClaimsAdjudicate)]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] UpdateClaimStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _claimService.UpdateStatusAsync(id, request, GetCurrentUserName(), cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:long}/documents")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)}",
        Policy = PermissionPolicies.ClaimsCreate)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ClaimDocumentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadDocument(
        long id,
        [FromForm] long documentTypeId,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest(new { error = "Document file is required." });
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var request = new UploadClaimDocumentRequestDto
        {
            DocumentTypeId = documentTypeId,
            FileName = file.FileName,
            Content = memoryStream.ToArray()
        };

        var created = await _claimService.UploadDocumentAsync(
            id,
            request,
            GetCurrentUserId(),
            GetCurrentUserName(),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPost("{id:long}/related-claims")]
    [Authorize(
        Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)}",
        Policy = PermissionPolicies.ClaimsCreate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LinkRelatedClaim(
        long id,
        [FromBody] LinkRelatedClaimRequestDto request,
        CancellationToken cancellationToken)
    {
        await _claimService.LinkRelatedClaimAsync(
            id,
            request.RelatedClaimId,
            GetCurrentUserId(),
            GetCurrentUserName(),
            cancellationToken);
        return NoContent();
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
