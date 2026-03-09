using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.BusinessRules;

public interface IClaimWorkflowEngine
{
    IReadOnlyList<ClaimStatus> GetAllowedTransitions(ClaimStatus currentStatus, Claim claim);
    void ValidateTransition(Claim claim, ClaimStatus currentStatus, ClaimStatus nextStatus);
    string GetDisplayName(ClaimStatus status);
}
