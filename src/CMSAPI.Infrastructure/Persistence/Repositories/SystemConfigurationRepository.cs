using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence.Repositories;

public sealed class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SystemConfigurationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClaimTypeMaster>> GetClaimTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimTypeMasters.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ClaimTypeName)
            .ToListAsync(cancellationToken);
    }

    public Task<ClaimTypeMaster?> GetClaimTypeByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dbContext.ClaimTypeMasters.FirstOrDefaultAsync(x => x.ClaimTypeId == id && x.IsActive, cancellationToken);

    public Task AddClaimTypeAsync(ClaimTypeMaster entity, CancellationToken cancellationToken = default) =>
        _dbContext.ClaimTypeMasters.AddAsync(entity, cancellationToken).AsTask();

    public void UpdateClaimType(ClaimTypeMaster entity) => _dbContext.ClaimTypeMasters.Update(entity);

    public async Task<IReadOnlyList<ClaimStatusMaster>> GetClaimStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimStatusMasters.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SequenceNo)
            .ToListAsync(cancellationToken);
    }

    public Task<ClaimStatusMaster?> GetClaimStatusByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dbContext.ClaimStatusMasters.FirstOrDefaultAsync(x => x.ClaimStatusId == id && x.IsActive, cancellationToken);

    public Task AddClaimStatusAsync(ClaimStatusMaster entity, CancellationToken cancellationToken = default) =>
        _dbContext.ClaimStatusMasters.AddAsync(entity, cancellationToken).AsTask();

    public void UpdateClaimStatus(ClaimStatusMaster entity) => _dbContext.ClaimStatusMasters.Update(entity);

    public async Task<IReadOnlyList<InsuranceProduct>> GetInsuranceProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceProducts.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.PolicyTypeName)
            .ToListAsync(cancellationToken);
    }

    public Task<InsuranceProduct?> GetInsuranceProductByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dbContext.InsuranceProducts.FirstOrDefaultAsync(x => x.PolicyTypeId == id && x.IsActive, cancellationToken);

    public Task AddInsuranceProductAsync(InsuranceProduct entity, CancellationToken cancellationToken = default) =>
        _dbContext.InsuranceProducts.AddAsync(entity, cancellationToken).AsTask();

    public void UpdateInsuranceProduct(InsuranceProduct entity) => _dbContext.InsuranceProducts.Update(entity);

    public async Task<IReadOnlyList<FraudRuleMaster>> GetFraudRulesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FraudRuleMasters.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RuleName)
            .ToListAsync(cancellationToken);
    }

    public Task<FraudRuleMaster?> GetFraudRuleByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dbContext.FraudRuleMasters.FirstOrDefaultAsync(x => x.FraudRuleId == id && x.IsActive, cancellationToken);

    public Task AddFraudRuleAsync(FraudRuleMaster entity, CancellationToken cancellationToken = default) =>
        _dbContext.FraudRuleMasters.AddAsync(entity, cancellationToken).AsTask();

    public void UpdateFraudRule(FraudRuleMaster entity) => _dbContext.FraudRuleMasters.Update(entity);

    public async Task<IReadOnlyList<WorkflowStageMaster>> GetWorkflowStagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkflowStageMasters.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.StageSequence)
            .ToListAsync(cancellationToken);
    }

    public Task<WorkflowStageMaster?> GetWorkflowStageByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dbContext.WorkflowStageMasters.FirstOrDefaultAsync(x => x.WorkflowStageId == id && x.IsActive, cancellationToken);

    public Task AddWorkflowStageAsync(WorkflowStageMaster entity, CancellationToken cancellationToken = default) =>
        _dbContext.WorkflowStageMasters.AddAsync(entity, cancellationToken).AsTask();

    public void UpdateWorkflowStage(WorkflowStageMaster entity) => _dbContext.WorkflowStageMasters.Update(entity);
}
