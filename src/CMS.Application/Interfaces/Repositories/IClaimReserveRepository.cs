using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Repositories;

public interface IClaimReserveRepository
{
    Task<ClaimReserveDto> CreateInitialReserveAsync(Guid claimId, decimal reserveAmount, string currencyCode, string? reason, Guid? createdByUserId, CancellationToken cancellationToken);
    Task<ClaimReserveHistoryDto> RequestReserveAdjustmentAsync(Guid claimId, decimal reserveAmount, string? reason, Guid? requestedByUserId, CancellationToken cancellationToken);
    Task<ClaimReserveDto?> GetClaimReserveAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveHistoryAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveApprovalQueueAsync(string? status, CancellationToken cancellationToken);
    Task ApproveReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
    Task RejectReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
}
