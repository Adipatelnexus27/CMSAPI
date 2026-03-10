using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Domain.Enums;

namespace CMS.Application.Services;

public sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;

    public ConfigurationService(IConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<IReadOnlyList<LookupConfigurationItemDto>> GetLookupItemsAsync(string configType, CancellationToken cancellationToken)
    {
        ValidateConfigType(configType);
        return await _configurationRepository.GetLookupItemsAsync(configType, cancellationToken);
    }

    public async Task<LookupConfigurationItemDto> CreateLookupItemAsync(string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        ValidateConfigType(configType);
        ValidateLookupRequest(request);
        return await _configurationRepository.CreateLookupItemAsync(configType, request, cancellationToken);
    }

    public async Task<LookupConfigurationItemDto> UpdateLookupItemAsync(Guid configurationItemId, string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        ValidateConfigType(configType);
        ValidateLookupRequest(request);
        return await _configurationRepository.UpdateLookupItemAsync(configurationItemId, configType, request, cancellationToken);
    }

    public async Task DeleteLookupItemAsync(Guid configurationItemId, CancellationToken cancellationToken)
    {
        await _configurationRepository.DeleteLookupItemAsync(configurationItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<FraudDetectionRuleDto>> GetFraudRulesAsync(CancellationToken cancellationToken)
    {
        return await _configurationRepository.GetFraudRulesAsync(cancellationToken);
    }

    public async Task<FraudDetectionRuleDto> CreateFraudRuleAsync(UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        ValidateFraudRuleRequest(request);
        return await _configurationRepository.CreateFraudRuleAsync(request, cancellationToken);
    }

    public async Task<FraudDetectionRuleDto> UpdateFraudRuleAsync(Guid fraudRuleId, UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        ValidateFraudRuleRequest(request);
        return await _configurationRepository.UpdateFraudRuleAsync(fraudRuleId, request, cancellationToken);
    }

    public async Task DeleteFraudRuleAsync(Guid fraudRuleId, CancellationToken cancellationToken)
    {
        await _configurationRepository.DeleteFraudRuleAsync(fraudRuleId, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowSettingDto>> GetWorkflowSettingsAsync(CancellationToken cancellationToken)
    {
        return await _configurationRepository.GetWorkflowSettingsAsync(cancellationToken);
    }

    public async Task<WorkflowSettingDto> CreateWorkflowSettingAsync(UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWorkflowRequest(request);
        return await _configurationRepository.CreateWorkflowSettingAsync(request, cancellationToken);
    }

    public async Task<WorkflowSettingDto> UpdateWorkflowSettingAsync(Guid workflowSettingId, UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWorkflowRequest(request);
        return await _configurationRepository.UpdateWorkflowSettingAsync(workflowSettingId, request, cancellationToken);
    }

    public async Task DeleteWorkflowSettingAsync(Guid workflowSettingId, CancellationToken cancellationToken)
    {
        await _configurationRepository.DeleteWorkflowSettingAsync(workflowSettingId, cancellationToken);
    }

    private static void ValidateConfigType(string configType)
    {
        if (!ConfigurationTypes.All.Contains(configType))
        {
            throw new InvalidOperationException($"Unsupported config type: {configType}");
        }
    }

    private static void ValidateLookupRequest(UpsertLookupConfigurationItemRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Code is required.");
        if (request.DisplayOrder < 0) throw new InvalidOperationException("Display order must be non-negative.");
    }

    private static void ValidateFraudRuleRequest(UpsertFraudDetectionRuleRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RuleName)) throw new InvalidOperationException("Rule name is required.");
        if (string.IsNullOrWhiteSpace(request.RuleExpression)) throw new InvalidOperationException("Rule expression is required.");
        if (request.SeverityScore < 0 || request.SeverityScore > 100) throw new InvalidOperationException("Severity score must be between 0 and 100.");
        if (request.Priority < 0) throw new InvalidOperationException("Priority must be non-negative.");
    }

    private static void ValidateWorkflowRequest(UpsertWorkflowSettingRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.WorkflowKey)) throw new InvalidOperationException("Workflow key is required.");
        if (string.IsNullOrWhiteSpace(request.StepName)) throw new InvalidOperationException("Step name is required.");
        if (request.StepSequence <= 0) throw new InvalidOperationException("Step sequence must be greater than 0.");
        if (string.IsNullOrWhiteSpace(request.AssignedRole)) throw new InvalidOperationException("Assigned role is required.");
        if (request.SlaHours < 0) throw new InvalidOperationException("SLA hours must be non-negative.");
    }
}
