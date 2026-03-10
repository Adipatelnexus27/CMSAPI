using CMS.API.Middlewares;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/system-configuration")]
[Authorize(Roles = "Admin,Claim Manager")]
public sealed class SystemConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public SystemConfigurationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpGet("lookup/{configType}")]
    [RequirePermission("Config.View")]
    public async Task<IActionResult> GetLookupItems(string configType, CancellationToken cancellationToken)
    {
        var items = await _configurationService.GetLookupItemsAsync(configType, cancellationToken);
        return Ok(items);
    }

    [HttpPost("lookup/{configType}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> CreateLookupItem(string configType, [FromBody] UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        var item = await _configurationService.CreateLookupItemAsync(configType, request, cancellationToken);
        return Ok(item);
    }

    [HttpPut("lookup/{configType}/{configurationItemId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> UpdateLookupItem(string configType, Guid configurationItemId, [FromBody] UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        var item = await _configurationService.UpdateLookupItemAsync(configurationItemId, configType, request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("lookup/{configurationItemId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> DeleteLookupItem(Guid configurationItemId, CancellationToken cancellationToken)
    {
        await _configurationService.DeleteLookupItemAsync(configurationItemId, cancellationToken);
        return NoContent();
    }

    [HttpGet("fraud-rules")]
    [RequirePermission("Config.View")]
    public async Task<IActionResult> GetFraudRules(CancellationToken cancellationToken)
    {
        var rules = await _configurationService.GetFraudRulesAsync(cancellationToken);
        return Ok(rules);
    }

    [HttpPost("fraud-rules")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> CreateFraudRule([FromBody] UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        var rule = await _configurationService.CreateFraudRuleAsync(request, cancellationToken);
        return Ok(rule);
    }

    [HttpPut("fraud-rules/{fraudRuleId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> UpdateFraudRule(Guid fraudRuleId, [FromBody] UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        var rule = await _configurationService.UpdateFraudRuleAsync(fraudRuleId, request, cancellationToken);
        return Ok(rule);
    }

    [HttpDelete("fraud-rules/{fraudRuleId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> DeleteFraudRule(Guid fraudRuleId, CancellationToken cancellationToken)
    {
        await _configurationService.DeleteFraudRuleAsync(fraudRuleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("workflow-settings")]
    [RequirePermission("Config.View")]
    public async Task<IActionResult> GetWorkflowSettings(CancellationToken cancellationToken)
    {
        var settings = await _configurationService.GetWorkflowSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPost("workflow-settings")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> CreateWorkflowSetting([FromBody] UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        var setting = await _configurationService.CreateWorkflowSettingAsync(request, cancellationToken);
        return Ok(setting);
    }

    [HttpPut("workflow-settings/{workflowSettingId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> UpdateWorkflowSetting(Guid workflowSettingId, [FromBody] UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        var setting = await _configurationService.UpdateWorkflowSettingAsync(workflowSettingId, request, cancellationToken);
        return Ok(setting);
    }

    [HttpDelete("workflow-settings/{workflowSettingId:guid}")]
    [RequirePermission("Config.Manage")]
    public async Task<IActionResult> DeleteWorkflowSetting(Guid workflowSettingId, CancellationToken cancellationToken)
    {
        await _configurationService.DeleteWorkflowSettingAsync(workflowSettingId, cancellationToken);
        return NoContent();
    }
}
