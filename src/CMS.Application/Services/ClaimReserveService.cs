using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;

namespace CMS.Application.Services;

public sealed class ClaimReserveService : IClaimReserveService
{
    private static readonly HashSet<string> AllowedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "PendingApproval",
        "Approved",
        "Rejected"
    };

    private readonly IClaimReserveRepository _claimReserveRepository;
    private readonly IClaimRepository _claimRepository;

    public ClaimReserveService(IClaimReserveRepository claimReserveRepository, IClaimRepository claimRepository)
    {
        _claimReserveRepository = claimReserveRepository;
        _claimRepository = claimRepository;
    }

    public async Task<ClaimReserveDto> CreateInitialReserveAsync(Guid claimId, CreateInitialReserveRequestDto request, Guid? createdByUserId, CancellationToken cancellationToken)
    {
        ValidateReserveAmount(request.ReserveAmount);

        var currencyCode = NormalizeCurrencyCode(request.CurrencyCode);
        await EnsureClaimExists(claimId, cancellationToken);

        return await _claimReserveRepository.CreateInitialReserveAsync(
            claimId,
            request.ReserveAmount,
            currencyCode,
            NormalizeOptionalText(request.Reason),
            createdByUserId,
            cancellationToken);
    }

    public async Task<ClaimReserveHistoryDto> AdjustReserveAsync(Guid claimId, AdjustReserveRequestDto request, Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        ValidateReserveAmount(request.ReserveAmount);
        await EnsureClaimExists(claimId, cancellationToken);

        return await _claimReserveRepository.RequestReserveAdjustmentAsync(
            claimId,
            request.ReserveAmount,
            NormalizeOptionalText(request.Reason),
            requestedByUserId,
            cancellationToken);
    }

    public async Task<ClaimReserveDto> GetClaimReserveAsync(Guid claimId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        var reserve = await _claimReserveRepository.GetClaimReserveAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim reserve not found.");

        return reserve;
    }

    public async Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveHistoryAsync(Guid claimId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        return await _claimReserveRepository.GetReserveHistoryAsync(claimId, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimReserveHistoryDto>> GetReserveApprovalQueueAsync(string? status, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(status) && !AllowedApprovalStatuses.Contains(status.Trim()))
        {
            throw new InvalidOperationException("Status must be one of: PendingApproval, Approved, Rejected.");
        }

        return await _claimReserveRepository.GetReserveApprovalQueueAsync(NormalizeOptionalText(status), cancellationToken);
    }

    public async Task ApproveReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        if (claimReserveHistoryId == Guid.Empty)
        {
            throw new InvalidOperationException("Reserve history id is required.");
        }

        await _claimReserveRepository.ApproveReserveAdjustmentAsync(
            claimReserveHistoryId,
            NormalizeOptionalText(approvalNote),
            approvedByUserId,
            cancellationToken);
    }

    public async Task RejectReserveAdjustmentAsync(Guid claimReserveHistoryId, string? approvalNote, Guid? approvedByUserId, CancellationToken cancellationToken)
    {
        if (claimReserveHistoryId == Guid.Empty)
        {
            throw new InvalidOperationException("Reserve history id is required.");
        }

        await _claimReserveRepository.RejectReserveAdjustmentAsync(
            claimReserveHistoryId,
            NormalizeOptionalText(approvalNote),
            approvedByUserId,
            cancellationToken);
    }

    private async Task EnsureClaimExists(Guid claimId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        _ = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");
    }

    private static void ValidateReserveAmount(decimal reserveAmount)
    {
        if (reserveAmount <= 0)
        {
            throw new InvalidOperationException("Reserve amount must be greater than 0.");
        }

        if (reserveAmount > 1000000000)
        {
            throw new InvalidOperationException("Reserve amount exceeds configured limit.");
        }
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

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
