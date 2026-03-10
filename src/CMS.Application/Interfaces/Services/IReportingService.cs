using CMS.Application.DTOs;
using CMS.Application.Models;

namespace CMS.Application.Interfaces.Services;

public interface IReportingService
{
    Task<IReadOnlyList<ClaimsByStatusReportDto>> GetClaimsByStatusAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimsByProductReportDto>> GetClaimsByProductAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudReportDto>> GetFraudReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvestigatorPerformanceReportDto>> GetInvestigatorPerformanceReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<SettlementReportDto>> GetSettlementReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<ReportExportFile> ExportExcelAsync(string reportType, DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<ReportExportFile> ExportPdfAsync(string reportType, DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
}
