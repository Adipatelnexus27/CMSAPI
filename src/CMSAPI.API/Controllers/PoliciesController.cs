using CMSAPI.Application.DTOs.Policies;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMSAPI.API.Controllers;

[ApiController]
[Route("policies")]
[Authorize]
public sealed class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)}")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var policies = await _policyService.GetAllAsync(cancellationToken);
        return Ok(policies);
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var policy = await _policyService.GetByIdAsync(id, cancellationToken);
        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpGet("{policyNumber}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)},{nameof(UserRole.Adjuster)},{nameof(UserRole.Investigator)}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
    {
        var policy = await _policyService.GetByPolicyNumberAsync(policyNumber, cancellationToken);
        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.ClaimManager)}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePolicyRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _policyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.PolicyId }, created);
    }
}
