using CMSAPI.Application.DTOs.Policies;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using FluentValidation;

namespace CMSAPI.Application.Services;

public sealed class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePolicyRequestDto> _validator;

    public PolicyService(
        IPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreatePolicyRequestDto> validator)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PolicyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var policies = await _policyRepository.GetAllAsync(cancellationToken);
        if (policies.Count == 0)
        {
            return [];
        }

        var result = new List<PolicyDto>(policies.Count);
        foreach (var policy in policies)
        {
            result.Add(await BuildPolicyDtoAsync(policy, cancellationToken));
        }

        return result;
    }

    public async Task<PolicyDto?> GetByIdAsync(long policyId, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId, cancellationToken);
        if (policy is null)
        {
            return null;
        }

        return await BuildPolicyDtoAsync(policy, cancellationToken);
    }

    public async Task<PolicyDto?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetByPolicyNumberAsync(policyNumber, cancellationToken);
        if (policy is null)
        {
            return null;
        }

        return await BuildPolicyDtoAsync(policy, cancellationToken);
    }

    public async Task<PolicyDto> CreateAsync(CreatePolicyRequestDto request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _policyRepository.ExistsByPolicyNumberAsync(request.PolicyNumber.Trim(), cancellationToken))
        {
            throw new InvalidOperationException($"Policy number '{request.PolicyNumber}' already exists.");
        }

        var now = DateTime.UtcNow;
        var policy = new Policy
        {
            PolicyNumber = request.PolicyNumber.Trim(),
            PolicyTypeId = request.PolicyTypeId,
            InsuredName = request.InsuredName.Trim(),
            PolicyStartDate = request.PolicyStartDate,
            PolicyEndDate = request.PolicyEndDate,
            SumInsured = request.SumInsured,
            CurrencyId = request.CurrencyId,
            PolicyStatus = request.PolicyStatus.Trim(),
            CreatedDate = now,
            CreatedBy = "api"
        };

        await _policyRepository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var coverageRequest in request.Coverages)
        {
            var coverage = new PolicyCoverage
            {
                PolicyId = policy.PolicyId,
                CoverageTypeId = coverageRequest.CoverageTypeId,
                CoverageLimit = coverageRequest.CoverageLimit,
                DeductibleAmount = coverageRequest.DeductibleAmount,
                EffectiveFrom = coverageRequest.EffectiveFrom,
                EffectiveTo = coverageRequest.EffectiveTo,
                CreatedDate = now,
                CreatedBy = "api"
            };

            await _policyRepository.AddCoverageAsync(coverage, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await BuildPolicyDtoAsync(policy, cancellationToken);
    }

    private async Task<PolicyDto> BuildPolicyDtoAsync(Policy policy, CancellationToken cancellationToken)
    {
        var coverages = await _policyRepository.GetCoveragesByPolicyIdAsync(policy.PolicyId, cancellationToken);
        var coverageTypeNames = await _policyRepository.GetCoverageTypeNamesAsync(
            coverages.Select(x => x.CoverageTypeId),
            cancellationToken);

        return new PolicyDto
        {
            PolicyId = policy.PolicyId,
            PolicyNumber = policy.PolicyNumber,
            PolicyTypeId = policy.PolicyTypeId,
            InsuredName = policy.InsuredName,
            PolicyStartDate = policy.PolicyStartDate,
            PolicyEndDate = policy.PolicyEndDate,
            SumInsured = policy.SumInsured,
            CurrencyId = policy.CurrencyId,
            PolicyStatus = policy.PolicyStatus,
            Coverages = coverages
                .Select(x => new PolicyCoverageDto
                {
                    PolicyCoverageId = x.PolicyCoverageId,
                    CoverageTypeId = x.CoverageTypeId,
                    CoverageTypeName = coverageTypeNames.TryGetValue(x.CoverageTypeId, out var name) ? name : string.Empty,
                    CoverageLimit = x.CoverageLimit,
                    DeductibleAmount = x.DeductibleAmount,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo
                })
                .ToList()
        };
    }
}

