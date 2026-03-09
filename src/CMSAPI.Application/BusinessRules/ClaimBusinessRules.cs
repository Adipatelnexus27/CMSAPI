using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;

namespace CMSAPI.Application.BusinessRules;

public sealed class ClaimBusinessRules
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IClaimWorkflowEngine _workflowEngine;

    public ClaimBusinessRules(IClaimRepository claimRepository, IPolicyRepository policyRepository, IClaimWorkflowEngine workflowEngine)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _workflowEngine = workflowEngine;
    }

    public async Task EnsureClaimNumberIsUniqueAsync(string claimNumber, CancellationToken cancellationToken)
    {
        if (await _claimRepository.ExistsByClaimNumberAsync(claimNumber, cancellationToken))
        {
            throw new InvalidOperationException($"Claim number '{claimNumber}' already exists.");
        }
    }

    public void EnsureStatusTransitionAllowed(Claim claim, ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        _workflowEngine.ValidateTransition(claim, currentStatus, nextStatus);
    }

    public IReadOnlyList<ClaimStatus> GetAllowedTransitions(Claim claim, ClaimStatus currentStatus)
    {
        return _workflowEngine.GetAllowedTransitions(currentStatus, claim);
    }

    public string GetStatusDisplayName(ClaimStatus status)
    {
        return _workflowEngine.GetDisplayName(status);
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
