using CMSAPI.Domain.Entities;

namespace CMSAPI.Domain.Interfaces;

public interface IPolicyRepository
{
    Task<IReadOnlyList<Policy>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Policy?> GetByIdAsync(long policyId, CancellationToken cancellationToken = default);
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, string>> GetPolicyNumbersByIdsAsync(IEnumerable<long> policyIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyCoverage>> GetCoveragesByPolicyIdAsync(long policyId, CancellationToken cancellationToken = default);
    Task AddCoverageAsync(PolicyCoverage coverage, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, string>> GetCoverageTypeNamesAsync(IEnumerable<long> coverageTypeIds, CancellationToken cancellationToken = default);
}
