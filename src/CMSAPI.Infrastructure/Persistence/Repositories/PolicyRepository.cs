using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PolicyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Policy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Policies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Policy?> GetByIdAsync(long policyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PolicyId == policyId && x.IsActive, cancellationToken);
    }

    public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PolicyNumber == policyNumber && x.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Policies.AnyAsync(x => x.PolicyNumber == policyNumber, cancellationToken);
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _dbContext.Policies.AddAsync(policy, cancellationToken);
    }

    public async Task<IReadOnlyList<PolicyCoverage>> GetCoveragesByPolicyIdAsync(long policyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PolicyCoverages
            .AsNoTracking()
            .Where(x => x.PolicyId == policyId && x.IsActive)
            .OrderBy(x => x.PolicyCoverageId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddCoverageAsync(PolicyCoverage coverage, CancellationToken cancellationToken = default)
    {
        await _dbContext.PolicyCoverages.AddAsync(coverage, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<long, string>> GetCoverageTypeNamesAsync(IEnumerable<long> coverageTypeIds, CancellationToken cancellationToken = default)
    {
        var ids = coverageTypeIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, string>();
        }

        return await _dbContext.CoverageTypes
            .AsNoTracking()
            .Where(x => ids.Contains(x.CoverageTypeId))
            .ToDictionaryAsync(x => x.CoverageTypeId, x => x.CoverageName, cancellationToken);
    }
}

