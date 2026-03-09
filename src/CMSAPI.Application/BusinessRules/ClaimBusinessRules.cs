using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Interfaces;

namespace CMSAPI.Application.BusinessRules;

public sealed class ClaimBusinessRules
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> AllowedTransitions = new()
    {
        [ClaimStatus.Registered] = [ClaimStatus.PolicyValidated, ClaimStatus.Closed],
        [ClaimStatus.PolicyValidated] = [ClaimStatus.Assigned, ClaimStatus.Closed],
        [ClaimStatus.Assigned] = [ClaimStatus.InvestigationInProgress, ClaimStatus.Closed],
        [ClaimStatus.InvestigationInProgress] = [ClaimStatus.CoverageDetermined, ClaimStatus.Closed],
        [ClaimStatus.CoverageDetermined] = [ClaimStatus.LiabilityDetermined, ClaimStatus.Closed],
        [ClaimStatus.LiabilityDetermined] = [ClaimStatus.Reserved, ClaimStatus.Closed],
        [ClaimStatus.Reserved] = [ClaimStatus.SettlementProcessed, ClaimStatus.Closed],
        [ClaimStatus.SettlementProcessed] = [ClaimStatus.PaymentCompleted, ClaimStatus.Closed],
        [ClaimStatus.PaymentCompleted] = [ClaimStatus.Closed],
        [ClaimStatus.Closed] = []
    };

    private readonly IClaimRepository _claimRepository;

    public ClaimBusinessRules(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task EnsureClaimNumberIsUniqueAsync(string claimNumber, CancellationToken cancellationToken)
    {
        if (await _claimRepository.ExistsByClaimNumberAsync(claimNumber, cancellationToken))
        {
            throw new InvalidOperationException($"Claim number '{claimNumber}' already exists.");
        }
    }

    public void EnsureStatusTransitionAllowed(ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(nextStatus))
        {
            throw new InvalidOperationException($"Invalid claim status transition: {currentStatus} -> {nextStatus}.");
        }
    }
}

