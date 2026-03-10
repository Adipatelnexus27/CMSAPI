using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IClaimReserveService
{
    Task<ClaimReserveDto> CreateInitialReserveAsync(Guid claimId, CreateInitialReserveRequestDto request, Guid? createdByUserId, CancellationToken cancellationToken);
    Task<ClaimReserveHistoryDto> AdjustReserveAsync(Guid claimId, AdjustReserveRequestDto request, Guid? requestedByUserId, CancellationToken cancellationToken);
    Task<ClaimReserveDto> GetClaimReserveAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveHistoryAsync(Guid claimId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveApprovalQueueAsync(string? status, CancellationToken cancellationToken);
    Task ApproveReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
    Task RejectReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
}
