using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Application.Models;

namespace CMS.Application.Services;

public sealed class FraudRuleEngine : IFraudRuleEngine
{
    private static readonly string[] SuspiciousKeywords =
    [
        "cash",
        "stolen",
        "unknown",
        "night",
        "injury",
        "urgent",
        "fire",
        "collision",
        "total loss"
    ];

    private readonly IClaimRepository _claimRepository;
    private readonly IConfigurationRepository _configurationRepository;

    public FraudRuleEngine(IClaimRepository claimRepository, IConfigurationRepository configurationRepository)
    {
        _claimRepository = claimRepository;
        _configurationRepository = configurationRepository;
    }

    public async Task<IReadOnlyList<FraudDetectionResult>> EvaluateClaimAsync(Guid claimId, CancellationToken cancellationToken)
    {
        if (claimId == Guid.Empty)
        {
            throw new InvalidOperationException("Claim id is required.");
        }

        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var allClaims = await _claimRepository.GetClaimsAsync(cancellationToken);
        var activeRules = (await _configurationRepository.GetFraudRulesAsync(cancellationToken))
            .Where(rule => rule.IsActive)
            .OrderBy(rule => rule.Priority)
            .ToList();

        var results = new List<FraudDetectionResult>();

        var duplicateClaims = allClaims
            .Where(item => item.ClaimId != claimId
                && string.Equals(item.PolicyNumber, claim.PolicyNumber, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.ClaimType, claim.ClaimType, StringComparison.OrdinalIgnoreCase)
                && Math.Abs((item.IncidentDateUtc - claim.IncidentDateUtc).TotalHours) <= 48)
            .ToList();

        var duplicateDetected = duplicateClaims.Count > 0;
        if (duplicateDetected)
        {
            var duplicateNumbers = string.Join(", ", duplicateClaims.Take(3).Select(item => item.ClaimNumber));
            results.Add(new FraudDetectionResult
            {
                FlagType = "DuplicateClaim",
                SeverityScore = 90,
                Reason = $"Possible duplicate against claim(s): {duplicateNumbers}.",
                IsDuplicate = true,
                IsSuspicious = false
            });
        }

        var suspiciousSignals = new List<string>();
        var incidentDescription = claim.IncidentDescription ?? string.Empty;

        var matchedKeywords = SuspiciousKeywords
            .Where(keyword => incidentDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchedKeywords.Count > 0)
        {
            suspiciousSignals.Add($"Description contains suspicious keywords: {string.Join(", ", matchedKeywords)}");
        }

        var reporterClaimCount = allClaims.Count(item =>
            string.Equals(item.ReporterName, claim.ReporterName, StringComparison.OrdinalIgnoreCase)
            && item.CreatedAtUtc >= DateTime.UtcNow.AddDays(-30));

        if (reporterClaimCount >= 3)
        {
            suspiciousSignals.Add($"Reporter has {reporterClaimCount} claim(s) in the last 30 days.");
        }

        if (claim.Priority >= 5)
        {
            suspiciousSignals.Add("Claim priority is at maximum level.");
        }

        var suspiciousDetected = suspiciousSignals.Count > 0;
        if (suspiciousDetected)
        {
            var severity = Math.Min(95, 60 + suspiciousSignals.Count * 10);
            results.Add(new FraudDetectionResult
            {
                FlagType = "SuspiciousClaim",
                SeverityScore = severity,
                Reason = string.Join(" ", suspiciousSignals),
                IsDuplicate = false,
                IsSuspicious = true
            });
        }

        foreach (var rule in activeRules)
        {
            if (!EvaluateRuleExpression(rule.RuleExpression, claim, duplicateDetected, suspiciousDetected, reporterClaimCount))
            {
                continue;
            }

            results.Add(new FraudDetectionResult
            {
                FlagType = "RuleBased",
                RuleName = rule.RuleName,
                SeverityScore = rule.SeverityScore,
                Reason = $"Rule triggered: {rule.RuleName} ({rule.RuleExpression})",
                IsDuplicate = false,
                IsSuspicious = false
            });
        }

        return results
            .GroupBy(item => $"{item.FlagType}|{item.RuleName}|{item.Reason}")
            .Select(group => group.First())
            .ToList();
    }

    private static bool EvaluateRuleExpression(
        string expression,
        ClaimDetailDto claim,
        bool duplicateDetected,
        bool suspiciousDetected,
        int reporterClaimCount)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var normalized = expression.Trim();

        if (TryGetExpressionValue(normalized, "DESCRIPTION_CONTAINS", out var descriptionContainsValue))
        {
            return claim.IncidentDescription.Contains(descriptionContainsValue, StringComparison.OrdinalIgnoreCase);
        }

        if (TryGetExpressionValue(normalized, "CLAIM_TYPE_EQUALS", out var claimTypeValue))
        {
            return string.Equals(claim.ClaimType, claimTypeValue, StringComparison.OrdinalIgnoreCase);
        }

        if (TryGetExpressionValue(normalized, "STATUS_EQUALS", out var statusValue))
        {
            return string.Equals(claim.ClaimStatus, statusValue, StringComparison.OrdinalIgnoreCase);
        }

        if (TryGetExpressionValue(normalized, "REPORTER_CONTAINS", out var reporterContainsValue))
        {
            return claim.ReporterName.Contains(reporterContainsValue, StringComparison.OrdinalIgnoreCase);
        }

        if (TryGetExpressionValue(normalized, "PRIORITY_GTE", out var priorityValue)
            && int.TryParse(priorityValue, out var priorityThreshold))
        {
            return claim.Priority >= priorityThreshold;
        }

        if (TryGetExpressionValue(normalized, "REPORTER_CLAIMS_GTE", out var reporterClaimCountValue)
            && int.TryParse(reporterClaimCountValue, out var reporterClaimThreshold))
        {
            return reporterClaimCount >= reporterClaimThreshold;
        }

        if (string.Equals(normalized, "DUPLICATE_EXISTS", StringComparison.OrdinalIgnoreCase))
        {
            return duplicateDetected;
        }

        if (string.Equals(normalized, "SUSPICIOUS_SIGNAL_EXISTS", StringComparison.OrdinalIgnoreCase))
        {
            return suspiciousDetected;
        }

        return claim.IncidentDescription.Contains(normalized, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetExpressionValue(string expression, string key, out string value)
    {
        var prefix = key + ":";
        if (expression.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = expression[prefix.Length..].Trim();
            return value.Length > 0;
        }

        value = string.Empty;
        return false;
    }
}
