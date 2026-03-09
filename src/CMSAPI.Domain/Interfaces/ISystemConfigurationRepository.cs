using CMSAPI.Domain.Entities;

namespace CMSAPI.Domain.Interfaces;

public interface ISystemConfigurationRepository
{
    Task<IReadOnlyList<ClaimTypeMaster>> GetClaimTypesAsync(CancellationToken cancellationToken = default);
    Task<ClaimTypeMaster?> GetClaimTypeByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddClaimTypeAsync(ClaimTypeMaster entity, CancellationToken cancellationToken = default);
    void UpdateClaimType(ClaimTypeMaster entity);

    Task<IReadOnlyList<ClaimStatusMaster>> GetClaimStatusesAsync(CancellationToken cancellationToken = default);
    Task<ClaimStatusMaster?> GetClaimStatusByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddClaimStatusAsync(ClaimStatusMaster entity, CancellationToken cancellationToken = default);
    void UpdateClaimStatus(ClaimStatusMaster entity);

    Task<IReadOnlyList<InsuranceProduct>> GetInsuranceProductsAsync(CancellationToken cancellationToken = default);
    Task<InsuranceProduct?> GetInsuranceProductByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddInsuranceProductAsync(InsuranceProduct entity, CancellationToken cancellationToken = default);
    void UpdateInsuranceProduct(InsuranceProduct entity);

    Task<IReadOnlyList<FraudRuleMaster>> GetFraudRulesAsync(CancellationToken cancellationToken = default);
    Task<FraudRuleMaster?> GetFraudRuleByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddFraudRuleAsync(FraudRuleMaster entity, CancellationToken cancellationToken = default);
    void UpdateFraudRule(FraudRuleMaster entity);

    Task<IReadOnlyList<WorkflowStageMaster>> GetWorkflowStagesAsync(CancellationToken cancellationToken = default);
    Task<WorkflowStageMaster?> GetWorkflowStageByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddWorkflowStageAsync(WorkflowStageMaster entity, CancellationToken cancellationToken = default);
    void UpdateWorkflowStage(WorkflowStageMaster entity);
}

