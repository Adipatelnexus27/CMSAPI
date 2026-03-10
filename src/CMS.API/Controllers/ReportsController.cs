using CMS.API.Middlewares;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("claims-by-status")]
    [RequirePermission("Reports.Analytics.Read")]
    public async Task<IActionResult> GetClaimsByStatus([FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var rows = await _reportingService.GetClaimsByStatusAsync(fromDateUtc, toDateUtc, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("claims-by-product")]
    [RequirePermission("Reports.Analytics.Read")]
    public async Task<IActionResult> GetClaimsByProduct([FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var rows = await _reportingService.GetClaimsByProductAsync(fromDateUtc, toDateUtc, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("fraud")]
    [RequirePermission("Reports.Analytics.Read")]
    public async Task<IActionResult> GetFraudReport([FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var rows = await _reportingService.GetFraudReportAsync(fromDateUtc, toDateUtc, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("investigator-performance")]
    [RequirePermission("Reports.Analytics.Read")]
    public async Task<IActionResult> GetInvestigatorPerformance([FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var rows = await _reportingService.GetInvestigatorPerformanceReportAsync(fromDateUtc, toDateUtc, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("settlements")]
    [RequirePermission("Reports.Analytics.Read")]
    public async Task<IActionResult> GetSettlementReport([FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var rows = await _reportingService.GetSettlementReportAsync(fromDateUtc, toDateUtc, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("export/excel")]
    [RequirePermission("Reports.Export.Excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string reportType, [FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var file = await _reportingService.ExportExcelAsync(reportType, fromDateUtc, toDateUtc, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("export/pdf")]
    [RequirePermission("Reports.Export.PDF")]
    public async Task<IActionResult> ExportPdf([FromQuery] string reportType, [FromQuery] DateTime? fromDateUtc, [FromQuery] DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        var file = await _reportingService.ExportPdfAsync(reportType, fromDateUtc, toDateUtc, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
