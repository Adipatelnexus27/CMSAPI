using CMS.Application.DTOs;
using CMS.Application.Models;

namespace CMS.Application.Interfaces.Services;

public interface IAuditService
{
    Task CreateAuditLogAsync(
        CreateAuditLogRequestDto request,
        Guid? userId,
        string? userEmail,
        string? userRoleCsv,
        string? ipAddress,
        string? userAgent,
        Guid? correlationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken);
    Task<AuditReportSummaryDto> GetAuditReportAsync(AuditLogFilterDto filter, CancellationToken cancellationToken);
    Task<ReportExportFile> ExportExcelAsync(AuditLogFilterDto filter, CancellationToken cancellationToken);
    Task<ReportExportFile> ExportPdfAsync(AuditLogFilterDto filter, CancellationToken cancellationToken);
}
