using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IReportingRepository
{
    Task<IReadOnlyList<ClaimsByStatusReportDto>> GetClaimsByStatusAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimsByProductReportDto>> GetClaimsByProductAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudReportDto>> GetFraudReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvestigatorPerformanceReportDto>> GetInvestigatorPerformanceReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<SettlementReportDto>> GetSettlementReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken);
}
