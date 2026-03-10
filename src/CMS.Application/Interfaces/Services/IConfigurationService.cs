using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IConfigurationService
{
    Task<IReadOnlyList<LookupConfigurationItemDto>> GetLookupItemsAsync(string configType, CancellationToken cancellationToken);
    Task<LookupConfigurationItemDto> CreateLookupItemAsync(string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken);
    Task<LookupConfigurationItemDto> UpdateLookupItemAsync(Guid configurationItemId, string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken);
    Task DeleteLookupItemAsync(Guid configurationItemId, CancellationToken cancellationToken);

    Task<IReadOnlyList<FraudDetectionRuleDto>> GetFraudRulesAsync(CancellationToken cancellationToken);
    Task<FraudDetectionRuleDto> CreateFraudRuleAsync(UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken);
    Task<FraudDetectionRuleDto> UpdateFraudRuleAsync(Guid fraudRuleId, UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken);
    Task DeleteFraudRuleAsync(Guid fraudRuleId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkflowSettingDto>> GetWorkflowSettingsAsync(CancellationToken cancellationToken);
    Task<WorkflowSettingDto> CreateWorkflowSettingAsync(UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken);
    Task<WorkflowSettingDto> UpdateWorkflowSettingAsync(Guid workflowSettingId, UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken);
    Task DeleteWorkflowSettingAsync(Guid workflowSettingId, CancellationToken cancellationToken);
}
