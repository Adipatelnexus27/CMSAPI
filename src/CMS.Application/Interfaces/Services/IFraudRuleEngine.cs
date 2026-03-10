using CMS.Application.Models;

namespace CMS.Application.Interfaces.Services;

public interface IFraudRuleEngine
{
    Task<IReadOnlyList<FraudDetectionResult>> EvaluateClaimAsync(Guid claimId, CancellationToken cancellationToken);
}
