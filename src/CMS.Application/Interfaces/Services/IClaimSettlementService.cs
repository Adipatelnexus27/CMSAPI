using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IClaimSettlementService
{
    Task<ClaimSettlementDto> CalculateSettlementAsync(Guid claimId, CalculateSettlementRequestDto request, Guid? calculatedByUserId, CancellationToken cancellationToken);
    Task<ClaimSettlementDto> GetSettlementByClaimIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task<ClaimPaymentDto> RequestPaymentApprovalAsync(Guid claimSettlementId, RequestPaymentApprovalDto request, Guid? requestedByUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsAsync(string? paymentStatus, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken);
    Task ApprovePaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
    Task RejectPaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken);
    Task UpdatePaymentStatusAsync(Guid claimPaymentId, string paymentStatus, string? statusNote, Guid? changedByUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClaimPaymentStatusHistoryDto>> GetPaymentStatusHistoryAsync(Guid claimPaymentId, CancellationToken cancellationToken);
}
