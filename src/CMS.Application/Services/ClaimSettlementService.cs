using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;

namespace CMS.Application.Services;

public sealed class ClaimSettlementService : IClaimSettlementService
{
    private static readonly HashSet<string> AllowedPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "PendingApproval",
        "Approved",
        "Rejected",
        "Processing",
        "Paid",
        "Failed"
    };

    private static readonly HashSet<string> AllowedTrackingStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Processing",
        "Paid",
        "Failed"
    };

    private readonly IClaimSettlementRepository _claimSettlementRepository;
    private readonly IClaimRepository _claimRepository;

    public ClaimSettlementService(IClaimSettlementRepository claimSettlementRepository, IClaimRepository claimRepository)
    {
        _claimSettlementRepository = claimSettlementRepository;
        _claimRepository = claimRepository;
    }

    public async Task<ClaimSettlementDto> CalculateSettlementAsync(Guid claimId, CalculateSettlementRequestDto request, Guid? calculatedByUserId, CancellationToken cancellationToken)
    {
        ValidateClaimId(claimId);

        if (request.GrossLossAmount <= 0)
        {
            throw new InvalidOperationException("Gross loss amount must be greater than 0.");
        }

        if (request.GrossLossAmount > 1000000000)
        {
            throw new InvalidOperationException("Gross loss amount exceeds configured limit.");
        }

        var currencyCode = NormalizeCurrencyCode(request.CurrencyCode);
        await EnsureClaimExists(claimId, cancellationToken);

        return await _claimSettlementRepository.CalculateSettlementAsync(
            claimId,
            request.GrossLossAmount,
            currencyCode,
            calculatedByUserId,
            cancellationToken);
    }

    public async Task<ClaimSettlementDto> GetSettlementByClaimIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        ValidateClaimId(claimId);

        var settlement = await _claimSettlementRepository.GetSettlementByClaimIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim settlement not found.");

        return settlement;
    }

    public async Task<ClaimPaymentDto> RequestPaymentApprovalAsync(Guid claimSettlementId, RequestPaymentApprovalDto request, Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        if (claimSettlementId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim settlement id is required.");
        }

        if (request.PaymentAmount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than 0.");
        }

        if (request.PaymentAmount > 1000000000)
        {
            throw new InvalidOperationException("Payment amount exceeds configured limit.");
        }

        return await _claimSettlementRepository.RequestPaymentApprovalAsync(
            claimSettlementId,
            request.PaymentAmount,
            NormalizeOptionalText(request.RequestNote),
            requestedByUserId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsAsync(string? paymentStatus, CancellationToken cancellationToken)
    {
        var normalizedStatus = NormalizeOptionalText(paymentStatus);
        if (normalizedStatus is not null && !AllowedPaymentStatuses.Contains(normalizedStatus))
        {
            throw new InvalidOperationException("Payment status must be one of: PendingApproval, Approved, Rejected, Processing, Paid, Failed.");
        }

        return await _claimSettlementRepository.GetPaymentsAsync(normalizedStatus, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimPaymentDto>> GetPaymentsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        ValidateClaimId(claimId);
        return await _claimSettlementRepository.GetPaymentsByClaimIdAsync(claimId, cancellationToken);
    }

    public async Task ApprovePaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        ValidateClaimPaymentId(claimPaymentId);
        await _claimSettlementRepository.ApprovePaymentAsync(claimPaymentId, NormalizeOptionalText(approvalNote), approvedByUserId, cancellationToken);
    }

    public async Task RejectPaymentAsync(Guid claimPaymentId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        ValidateClaimPaymentId(claimPaymentId);
        await _claimSettlementRepository.RejectPaymentAsync(claimPaymentId, NormalizeOptionalText(approvalNote), approvedByUserId, cancellationToken);
    }

    public async Task UpdatePaymentStatusAsync(Guid claimPaymentId, string paymentStatus, string? statusNote, Guid? changedByUserId, CancellationToken cancellationToken)
    {
        ValidateClaimPaymentId(claimPaymentId);

        var normalizedStatus = NormalizeRequiredStatus(paymentStatus);
        if (!AllowedTrackingStatuses.Contains(normalizedStatus))
        {
            throw new InvalidOperationException("Payment status tracking only supports: Processing, Paid, Failed.");
        }

        await _claimSettlementRepository.UpdatePaymentStatusAsync(claimPaymentId, normalizedStatus, NormalizeOptionalText(statusNote), changedByUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimPaymentStatusHistoryDto>> GetPaymentStatusHistoryAsync(Guid claimPaymentId, CancellationToken cancellationToken)
    {
        ValidateClaimPaymentId(claimPaymentId);
        return await _claimSettlementRepository.GetPaymentStatusHistoryAsync(claimPaymentId, cancellationToken);
    }

    private async Task EnsureClaimExists(Guid claimId, CancellationToken cancellationToken)
    {
        _ = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = string.IsNullOrWhiteSpace(currencyCode) ? "USD" : currencyCode.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(ch => !char.IsLetter(ch)))
        {
            throw new InvalidOperationException("Currency code must be a 3-letter ISO code.");
        }

        return normalized;
    }

    private static string NormalizeRequiredStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new InvalidOperationException("Payment status is required.");
        }

        return status.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static void ValidateClaimId(Guid claimId)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }
    }

    private static void ValidateClaimPaymentId(Guid claimPaymentId)
    {
        if (claimPaymentId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim payment id is required.");
        }
    }
}
