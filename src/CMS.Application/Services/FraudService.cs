using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;

namespace CMS.Application.Services;

public sealed class FraudService : IFraudService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Open",
        "UnderInvestigation",
        "ConfirmedFraud",
        "Cleared"
    };

    private readonly IFraudRuleEngine _fraudRuleEngine;
    private readonly IFraudRepository _fraudRepository;

    public FraudService(IFraudRuleEngine fraudRuleEngine, IFraudRepository fraudRepository)
    {
        _fraudRuleEngine = fraudRuleEngine;
        _fraudRepository = fraudRepository;
    }

    public async Task<RunFraudDetectionResponseDto> RunDetectionAsync(Guid claimId, Guid? triggeredByUserId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        var evaluationResults = await _fraudRuleEngine.EvaluateClaimAsync(claimId, cancellationToken);
        var flags = new List<FraudFlagDto>();

        foreach (var result in evaluationResults)
        {
            var flag = await _fraudRepository.CreateOrReuseFraudFlagAsync(
                claimId,
                result.FlagType,
                result.RuleName,
                result.SeverityScore,
                result.Reason,
                result.IsDuplicate,
                result.IsSuspicious,
                triggeredByUserId,
                cancellationToken);

            flags.Add(flag);
        }

        return new RunFraudDetectionResponseDto
        {
            ClaimId = claimId,
            Flags = flags
        };
    }

    public async Task<IReadOnlyList<FraudFlagDto>> GetFraudFlagsAsync(string? status, CancellationToken cancellationToken)
    {
        ValidateStatusIfProvided(status);
        return await _fraudRepository.GetFraudFlagsAsync(status, cancellationToken);
    }

    public async Task<IReadOnlyList<FraudFlagDto>> GetClaimFraudFlagsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        return await _fraudRepository.GetFraudFlagsByClaimIdAsync(claimId, cancellationToken);
    }

    public async Task UpdateFraudFlagStatusAsync(Guid fraudFlagId, string status, string? reviewNote, Guid? reviewedByUserId, CancellationToken cancellationToken)
    {
        if (fraudFlagId == Guid.Empty)
        {
            throw new InvalidOperationException("Fraud flag id is required.");
        }

        if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status.Trim()))
        {
            throw new InvalidOperationException("Status must be one of: Open, UnderInvestigation, ConfirmedFraud, Cleared.");
        }

        await _fraudRepository.UpdateFraudFlagStatusAsync(fraudFlagId, status.Trim(), reviewNote?.Trim(), reviewedByUserId, cancellationToken);
    }

    private static void ValidateStatusIfProvided(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return;
        }

        if (!AllowedStatuses.Contains(status.Trim()))
        {
            throw new InvalidOperationException("Status filter must be one of: Open, UnderInvestigation, ConfirmedFraud, Cleared.");
        }
    }
}
