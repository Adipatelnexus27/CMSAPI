using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IAuditRepository
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
}
