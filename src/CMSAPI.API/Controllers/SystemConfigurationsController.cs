using CMSAPI.Application.DTOs.SystemConfiguration;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMSAPI.API.Controllers;

[ApiController]
[Route("api/system-config")]
[Authorize]
public sealed class SystemConfigurationsController : ControllerBase
{
    private readonly ISystemConfigurationService _service;

    public SystemConfigurationsController(ISystemConfigurationService service)
    {
        _service = service;
    }

    [HttpGet("claim-types")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetClaimTypes(CancellationToken cancellationToken) =>
        Ok(await _service.GetClaimTypesAsync(cancellationToken));

    [HttpGet("claim-types/{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetClaimTypeById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetClaimTypeByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("claim-types")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateClaimType([FromBody] UpsertClaimTypeRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateClaimTypeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClaimTypeById), new { id = item.ClaimTypeId }, item);
    }

    [HttpPut("claim-types/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateClaimType(long id, [FromBody] UpsertClaimTypeRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateClaimTypeAsync(id, request, cancellationToken));

    [HttpDelete("claim-types/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteClaimType(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteClaimTypeAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("claim-statuses")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetClaimStatuses(CancellationToken cancellationToken) =>
        Ok(await _service.GetClaimStatusesAsync(cancellationToken));

    [HttpGet("claim-statuses/{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetClaimStatusById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetClaimStatusByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("claim-statuses")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateClaimStatus([FromBody] UpsertClaimStatusRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateClaimStatusAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClaimStatusById), new { id = item.ClaimStatusId }, item);
    }

    [HttpPut("claim-statuses/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateClaimStatus(long id, [FromBody] UpsertClaimStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateClaimStatusAsync(id, request, cancellationToken));

    [HttpDelete("claim-statuses/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteClaimStatus(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteClaimStatusAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("insurance-products")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetInsuranceProducts(CancellationToken cancellationToken) =>
        Ok(await _service.GetInsuranceProductsAsync(cancellationToken));

    [HttpGet("insurance-products/{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetInsuranceProductById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetInsuranceProductByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("insurance-products")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateInsuranceProduct([FromBody] UpsertInsuranceProductRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateInsuranceProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetInsuranceProductById), new { id = item.PolicyTypeId }, item);
    }

    [HttpPut("insurance-products/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateInsuranceProduct(long id, [FromBody] UpsertInsuranceProductRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateInsuranceProductAsync(id, request, cancellationToken));

    [HttpDelete("insurance-products/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteInsuranceProduct(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteInsuranceProductAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("fraud-rules")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.FraudAnalyst)}")]
    public async Task<IActionResult> GetFraudRules(CancellationToken cancellationToken) =>
        Ok(await _service.GetFraudRulesAsync(cancellationToken));

    [HttpGet("fraud-rules/{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.FraudAnalyst)}")]
    public async Task<IActionResult> GetFraudRuleById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetFraudRuleByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("fraud-rules")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateFraudRule([FromBody] UpsertFraudRuleRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateFraudRuleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetFraudRuleById), new { id = item.FraudRuleId }, item);
    }

    [HttpPut("fraud-rules/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateFraudRule(long id, [FromBody] UpsertFraudRuleRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateFraudRuleAsync(id, request, cancellationToken));

    [HttpDelete("fraud-rules/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteFraudRule(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteFraudRuleAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("workflow-stages")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetWorkflowStages(CancellationToken cancellationToken) =>
        Ok(await _service.GetWorkflowStagesAsync(cancellationToken));

    [HttpGet("workflow-stages/{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    public async Task<IActionResult> GetWorkflowStageById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetWorkflowStageByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("workflow-stages")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateWorkflowStage([FromBody] UpsertWorkflowStageRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateWorkflowStageAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWorkflowStageById), new { id = item.WorkflowStageId }, item);
    }

    [HttpPut("workflow-stages/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateWorkflowStage(long id, [FromBody] UpsertWorkflowStageRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateWorkflowStageAsync(id, request, cancellationToken));

    [HttpDelete("workflow-stages/{id:long}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteWorkflowStage(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteWorkflowStageAsync(id, cancellationToken);
        return NoContent();
    }
}

