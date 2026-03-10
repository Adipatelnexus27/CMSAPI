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
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpPost]
    [RequirePermission("Audit.Log.Write")]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequestDto request, CancellationToken cancellationToken)
    {
        await _auditService.CreateAuditLogAsync(
            request,
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRolesCsv(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            GetCorrelationId(),
            cancellationToken);

        return Accepted();
    }

    [HttpGet]
    [RequirePermission("Audit.Log.Read")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var rows = await _auditService.GetAuditLogsAsync(filter, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("report")]
    [RequirePermission("Audit.Report.Read")]
    public async Task<IActionResult> GetAuditReport([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var summary = await _auditService.GetAuditReportAsync(filter, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("export/excel")]
    [RequirePermission("Audit.Export.Excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _auditService.ExportExcelAsync(filter, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("export/pdf")]
    [RequirePermission("Audit.Export.PDF")]
    public async Task<IActionResult> ExportPdf([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _auditService.ExportPdfAsync(filter, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private string? GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
    }

    private string? GetCurrentUserRolesCsv()
    {
        var roles = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return roles.Length == 0 ? null : string.Join(",", roles);
    }

    private Guid? GetCorrelationId()
    {
        if (Request.Headers.TryGetValue("X-Correlation-Id", out var correlationIdHeader)
            && Guid.TryParse(correlationIdHeader.ToString(), out var headerValue))
        {
            return headerValue;
        }

        return Guid.TryParse(HttpContext.TraceIdentifier, out var traceValue) ? traceValue : null;
    }
}
