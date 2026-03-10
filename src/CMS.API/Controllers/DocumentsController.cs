using System.Security.Claims;
using CMS.API.Middlewares;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("claims/{claimId:guid}/upload")]
    [RequirePermission("Documents.Upload")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(
        Guid claimId,
        [FromForm] string? documentCategory,
        [FromForm] Guid? documentGroupId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new InvalidOperationException("File is required.");
        }

        await using var stream = file.OpenReadStream();
        var response = await _documentService.UploadDocumentAsync(
            claimId,
            documentCategory,
            documentGroupId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            GetCurrentUserId(),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("claims/{claimId:guid}")]
    [RequirePermission("Documents.Read")]
    public async Task<IActionResult> GetClaimDocuments(Guid claimId, [FromQuery] bool latestOnly = true, CancellationToken cancellationToken = default)
    {
        var documents = await _documentService.GetClaimDocumentsAsync(claimId, latestOnly, cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{claimDocumentId:guid}")]
    [RequirePermission("Documents.Read")]
    public async Task<IActionResult> GetDocument(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentAsync(claimDocumentId, cancellationToken);
        return Ok(document);
    }

    [HttpGet("{claimDocumentId:guid}/versions")]
    [RequirePermission("Documents.Read")]
    public async Task<IActionResult> GetVersions(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var versions = await _documentService.GetDocumentVersionsAsync(claimDocumentId, cancellationToken);
        return Ok(versions);
    }

    [HttpGet("{claimDocumentId:guid}/preview")]
    [RequirePermission("Documents.Preview")]
    public async Task<IActionResult> Preview(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var preview = await _documentService.GetDocumentPreviewAsync(claimDocumentId, cancellationToken);
        Response.Headers.ContentDisposition = $"inline; filename=\"{SanitizeHeaderValue(preview.FileName)}\"";
        return File(preview.ContentStream, preview.ContentType);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private static string SanitizeHeaderValue(string input)
    {
        return input.Replace("\"", string.Empty, StringComparison.Ordinal);
    }
}
