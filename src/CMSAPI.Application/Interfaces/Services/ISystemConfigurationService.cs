using CMSAPI.Application.DTOs.SystemConfiguration;

namespace CMSAPI.Application.Interfaces.Services;

public interface ISystemConfigurationService
{
    Task<IReadOnlyList<ClaimTypeDto>> GetClaimTypesAsync(CancellationToken cancellationToken = default);
    Task<ClaimTypeDto?> GetClaimTypeByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ClaimTypeDto> CreateClaimTypeAsync(UpsertClaimTypeRequest request, CancellationToken cancellationToken = default);
    Task<ClaimTypeDto> UpdateClaimTypeAsync(long id, UpsertClaimTypeRequest request, CancellationToken cancellationToken = default);
    Task DeleteClaimTypeAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimStatusDto>> GetClaimStatusesAsync(CancellationToken cancellationToken = default);
    Task<ClaimStatusDto?> GetClaimStatusByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ClaimStatusDto> CreateClaimStatusAsync(UpsertClaimStatusRequest request, CancellationToken cancellationToken = default);
    Task<ClaimStatusDto> UpdateClaimStatusAsync(long id, UpsertClaimStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteClaimStatusAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InsuranceProductDto>> GetInsuranceProductsAsync(CancellationToken cancellationToken = default);
    Task<InsuranceProductDto?> GetInsuranceProductByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<InsuranceProductDto> CreateInsuranceProductAsync(UpsertInsuranceProductRequest request, CancellationToken cancellationToken = default);
    Task<InsuranceProductDto> UpdateInsuranceProductAsync(long id, UpsertInsuranceProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteInsuranceProductAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FraudRuleDto>> GetFraudRulesAsync(CancellationToken cancellationToken = default);
    Task<FraudRuleDto?> GetFraudRuleByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<FraudRuleDto> CreateFraudRuleAsync(UpsertFraudRuleRequest request, CancellationToken cancellationToken = default);
    Task<FraudRuleDto> UpdateFraudRuleAsync(long id, UpsertFraudRuleRequest request, CancellationToken cancellationToken = default);
    Task DeleteFraudRuleAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowStageDto>> GetWorkflowStagesAsync(CancellationToken cancellationToken = default);
    Task<WorkflowStageDto?> GetWorkflowStageByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkflowStageDto> CreateWorkflowStageAsync(UpsertWorkflowStageRequest request, CancellationToken cancellationToken = default);
    Task<WorkflowStageDto> UpdateWorkflowStageAsync(long id, UpsertWorkflowStageRequest request, CancellationToken cancellationToken = default);
    Task DeleteWorkflowStageAsync(long id, CancellationToken cancellationToken = default);
}

