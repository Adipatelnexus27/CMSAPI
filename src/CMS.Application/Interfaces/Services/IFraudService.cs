using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IFraudService
{
    Task<RunFraudDetectionResponseDto> RunDetectionAsync(Guid claimId, Guid? triggeredByUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsAsync(string? status, CancellationToken cancellationToken);
    Task<IReadOnlyList<FraudFlagDto>> GetClaimFraudFlagsAsync(Guid claimId, CancellationToken cancellationToken);
    Task UpdateFraudFlagStatusAsync(Guid fraudFlagId, string status, string? reviewNote, Guid? reviewedByUserId, CancellationToken cancellationToken);
}
