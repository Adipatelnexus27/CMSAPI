using CMS.API.Middlewares;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

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

    [HttpPost("register")]
    [RequirePermission("Claims.Create")]
    public async Task<IActionResult> Register([FromBody] CreateClaimRequestDto request, CancellationToken cancellationToken)
    {
        var claim = await _claimService.RegisterClaimAsync(request, cancellationToken);
        return Ok(claim);
    }

    [HttpGet]
    [RequirePermission("Claims.Read")]
    public async Task<IActionResult> GetClaims(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetClaimsAsync(cancellationToken);
        return Ok(claims);
    }

    [HttpGet("{claimId:guid}")]
    [RequirePermission("Claims.Read")]
    public async Task<IActionResult> GetClaim(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetClaimDetailAsync(claimId, cancellationToken);
        return Ok(claim);
    }

    [HttpPost("{claimId:guid}/documents")]
    [RequirePermission("Claims.DocumentsUpload")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(Guid claimId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new InvalidOperationException("File is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _claimService.UploadDocumentAsync(
            claimId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{claimId:guid}/related/{relatedClaimId:guid}")]
    [RequirePermission("Claims.Link")]
    public async Task<IActionResult> LinkRelated(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken)
    {
        await _claimService.LinkRelatedClaimAsync(claimId, relatedClaimId, cancellationToken);
        return NoContent();
    }
}
