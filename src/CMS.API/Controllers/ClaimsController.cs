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

    [HttpGet("assigned")]
    [RequirePermission("Claims.Assigned.Read")]
    public async Task<IActionResult> GetAssignedClaims([FromQuery] Guid assigneeUserId, [FromQuery] string role, CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetAssignedClaimsAsync(assigneeUserId, role, cancellationToken);
        return Ok(claims);
    }

    [HttpGet("investigation/dashboard")]
    [RequirePermission("Claims.Investigation.Read")]
    public async Task<IActionResult> GetInvestigationDashboard(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetInvestigationDashboardAsync(cancellationToken);
        return Ok(claims);
    }

    [HttpGet("{claimId:guid}")]
    [RequirePermission("Claims.Read")]
    public async Task<IActionResult> GetClaim(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetClaimDetailAsync(claimId, cancellationToken);
        return Ok(claim);
    }

    [HttpGet("{claimId:guid}/investigation")]
    [RequirePermission("Claims.Investigation.Read")]
    public async Task<IActionResult> GetClaimInvestigation(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetClaimInvestigationAsync(claimId, cancellationToken);
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
            GetCurrentUserId(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{claimId:guid}/investigation/documents")]
    [RequirePermission("Claims.Investigation.DocumentsUpload")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> UploadInvestigationDocument(Guid claimId, [FromForm] string documentCategory, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new InvalidOperationException("File is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _claimService.UploadInvestigationDocumentAsync(
            claimId,
            documentCategory,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            GetCurrentUserId(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{claimId:guid}/investigation/notes")]
    [RequirePermission("Claims.Investigation.Write")]
    public async Task<IActionResult> AddInvestigationNote(Guid claimId, [FromBody] AddInvestigatorNoteRequestDto request, CancellationToken cancellationToken)
    {
        var note = await _claimService.AddInvestigatorNoteAsync(claimId, request.NoteText, request.ProgressPercentSnapshot, GetCurrentUserId(), cancellationToken);
        return Ok(note);
    }

    [HttpPut("{claimId:guid}/investigation/progress")]
    [RequirePermission("Claims.Investigation.Write")]
    public async Task<IActionResult> UpdateInvestigationProgress(Guid claimId, [FromBody] UpdateInvestigationProgressRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.UpdateInvestigationProgressAsync(claimId, request.ProgressPercent, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{claimId:guid}/related/{relatedClaimId:guid}")]
    [RequirePermission("Claims.Link")]
    public async Task<IActionResult> LinkRelated(Guid claimId, Guid relatedClaimId, CancellationToken cancellationToken)
    {
        await _claimService.LinkRelatedClaimAsync(claimId, relatedClaimId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{claimId:guid}/assign/investigator")]
    [RequirePermission("Claims.AssignInvestigator")]
    public async Task<IActionResult> AssignInvestigator(Guid claimId, [FromBody] AssignInvestigatorRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.AssignInvestigatorAsync(claimId, request.InvestigatorUserId, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("{claimId:guid}/assign/adjuster")]
    [RequirePermission("Claims.AssignAdjuster")]
    public async Task<IActionResult> AssignAdjuster(Guid claimId, [FromBody] AssignAdjusterRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.AssignAdjusterAsync(claimId, request.AdjusterUserId, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("{claimId:guid}/priority")]
    [RequirePermission("Claims.SetPriority")]
    public async Task<IActionResult> SetPriority(Guid claimId, [FromBody] SetClaimPriorityRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.SetPriorityAsync(claimId, request.Priority, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("{claimId:guid}/status")]
    [RequirePermission("Claims.UpdateStatus")]
    public async Task<IActionResult> UpdateStatus(Guid claimId, [FromBody] UpdateClaimStatusRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.UpdateStatusAsync(claimId, request.ClaimStatus, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPut("{claimId:guid}/workflow-step")]
    [RequirePermission("Claims.UpdateWorkflow")]
    public async Task<IActionResult> UpdateWorkflowStep(Guid claimId, [FromBody] UpdateWorkflowStepRequestDto request, CancellationToken cancellationToken)
    {
        await _claimService.UpdateWorkflowStepAsync(claimId, request.WorkflowStep, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
