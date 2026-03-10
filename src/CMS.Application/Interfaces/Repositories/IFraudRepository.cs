using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IFraudRepository
{
    Task<FraudFlagDto> CreateOrReuseFraudFlagAsync(
        Guid claimId,
        string flagType,
        string? ruleName,
        int severityScore,
        string reason,
        bool isDuplicate,
        bool isSuspicious,
        Guid? createdByUserId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsAsync(string? status, CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task UpdateFraudFlagStatusAsync(Guid fraudFlagId, string status, string? reviewNote, Guid? reviewedByUserId, CancellationToken cancellationToken);
}
