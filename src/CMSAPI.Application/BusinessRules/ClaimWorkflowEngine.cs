using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.BusinessRules;

public sealed class ClaimWorkflowEngine : IClaimWorkflowEngine
{
    private static readonly IReadOnlyDictionary<ClaimStatus, IReadOnlyList<ClaimStatus>> TransitionMap =
        new Dictionary<ClaimStatus, IReadOnlyList<ClaimStatus>>
        {
            [ClaimStatus.New] = [ClaimStatus.Triage],
            [ClaimStatus.Triage] = [ClaimStatus.Assigned, ClaimStatus.FraudCheck, ClaimStatus.Closed],
            [ClaimStatus.Assigned] = [ClaimStatus.Investigation],
            [ClaimStatus.Investigation] = [ClaimStatus.CoverageReview, ClaimStatus.FraudCheck],
            [ClaimStatus.CoverageReview] = [ClaimStatus.LiabilityReview, ClaimStatus.Closed],
            [ClaimStatus.LiabilityReview] = [ClaimStatus.ReserveCreated],
            [ClaimStatus.ReserveCreated] = [ClaimStatus.FraudCheck, ClaimStatus.Settlement],
            [ClaimStatus.FraudCheck] = [ClaimStatus.Investigation, ClaimStatus.Settlement, ClaimStatus.Closed],
            [ClaimStatus.Settlement] = [ClaimStatus.Payment, ClaimStatus.Closed],
            [ClaimStatus.Payment] = [ClaimStatus.Closed],
            [ClaimStatus.Closed] = []
        };

    private static readonly IReadOnlyDictionary<ClaimStatus, string> DisplayNames = new Dictionary<ClaimStatus, string>
    {
        [ClaimStatus.New] = "New",
        [ClaimStatus.Triage] = "Triage",
        [ClaimStatus.Assigned] = "Assigned",
        [ClaimStatus.Investigation] = "Investigation",
        [ClaimStatus.CoverageReview] = "Coverage Review",
        [ClaimStatus.LiabilityReview] = "Liability Review",
        [ClaimStatus.ReserveCreated] = "Reserve Created",
        [ClaimStatus.FraudCheck] = "Fraud Check",
        [ClaimStatus.Settlement] = "Settlement",
        [ClaimStatus.Payment] = "Payment",
        [ClaimStatus.Closed] = "Closed"
    };

    public IReadOnlyList<ClaimStatus> GetAllowedTransitions(ClaimStatus currentStatus, Claim claim)
    {
        return TransitionMap.TryGetValue(currentStatus, out var next) ? next : [];
    }

    public void ValidateTransition(Claim claim, ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        var allowedStatuses = GetAllowedTransitions(currentStatus, claim);
        if (!allowedStatuses.Contains(nextStatus))
        {
            throw new InvalidOperationException(
                $"Invalid claim workflow transition: {GetDisplayName(currentStatus)} -> {GetDisplayName(nextStatus)}.");
        }

        switch (nextStatus)
        {
            case ClaimStatus.LiabilityReview when claim.EstimatedLossAmount <= 0:
                throw new InvalidOperationException("Estimated loss amount is required before Liability Review.");

            case ClaimStatus.ReserveCreated when claim.EstimatedLossAmount <= 0:
                throw new InvalidOperationException("Estimated loss amount must be greater than zero before Reserve Created.");

            case ClaimStatus.Settlement when claim.EstimatedLossAmount <= 0 && (!claim.ApprovedLossAmount.HasValue || claim.ApprovedLossAmount <= 0):
                throw new InvalidOperationException("Claim amount must be assessed before moving to Settlement.");

            case ClaimStatus.Payment when !claim.ApprovedLossAmount.HasValue || claim.ApprovedLossAmount <= 0:
                throw new InvalidOperationException("Approved loss amount must be greater than zero before Payment.");
        }
    }

    public string GetDisplayName(ClaimStatus status)
    {
        return DisplayNames.GetValueOrDefault(status, status.ToString());
    }
}
