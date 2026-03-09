using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Entities;
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
    private readonly IPolicyRepository _policyRepository;

    public ClaimBusinessRules(IClaimRepository claimRepository, IPolicyRepository policyRepository)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
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

    public async Task<Policy> EnsurePolicyIsEligibleForClaimAsync(string policyNumber, DateTime incidentDateUtc, CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByPolicyNumberAsync(policyNumber, cancellationToken);
        if (policy is null || !policy.IsActive)
        {
            throw new InvalidOperationException($"Policy '{policyNumber}' does not exist or is inactive.");
        }

        if (!policy.PolicyStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Policy '{policyNumber}' is not active.");
        }

        var incidentDate = incidentDateUtc.Date;
        if (incidentDate < policy.PolicyStartDate.Date || incidentDate > policy.PolicyEndDate.Date)
        {
            throw new InvalidOperationException($"Incident date '{incidentDate:yyyy-MM-dd}' is outside policy coverage period.");
        }

        return policy;
    }
}
