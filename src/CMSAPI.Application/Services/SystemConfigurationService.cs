using CMSAPI.Application.DTOs.SystemConfiguration;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using FluentValidation;

namespace CMSAPI.Application.Services;

public sealed class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpsertClaimTypeRequest> _claimTypeValidator;
    private readonly IValidator<UpsertClaimStatusRequest> _claimStatusValidator;
    private readonly IValidator<UpsertInsuranceProductRequest> _insuranceProductValidator;
    private readonly IValidator<UpsertFraudRuleRequest> _fraudRuleValidator;
    private readonly IValidator<UpsertWorkflowStageRequest> _workflowStageValidator;

    public SystemConfigurationService(
        ISystemConfigurationRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpsertClaimTypeRequest> claimTypeValidator,
        IValidator<UpsertClaimStatusRequest> claimStatusValidator,
        IValidator<UpsertInsuranceProductRequest> insuranceProductValidator,
        IValidator<UpsertFraudRuleRequest> fraudRuleValidator,
        IValidator<UpsertWorkflowStageRequest> workflowStageValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _claimTypeValidator = claimTypeValidator;
        _claimStatusValidator = claimStatusValidator;
        _insuranceProductValidator = insuranceProductValidator;
        _fraudRuleValidator = fraudRuleValidator;
        _workflowStageValidator = workflowStageValidator;
    }

    public async Task<IReadOnlyList<ClaimTypeDto>> GetClaimTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetClaimTypesAsync(cancellationToken);
        return items.Select(ToClaimTypeDto).ToList();
    }

    public async Task<ClaimTypeDto?> GetClaimTypeByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetClaimTypeByIdAsync(id, cancellationToken);
        return item is null ? null : ToClaimTypeDto(item);
    }

    public async Task<ClaimTypeDto> CreateClaimTypeAsync(UpsertClaimTypeRequest request, CancellationToken cancellationToken = default)
    {
        await _claimTypeValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = new ClaimTypeMaster
        {
            ClaimTypeCode = request.ClaimTypeCode.Trim(),
            ClaimTypeName = request.ClaimTypeName.Trim(),
            ClaimTypeDescription = request.ClaimTypeDescription?.Trim(),
            CreatedBy = "api"
        };
        await _repository.AddClaimTypeAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToClaimTypeDto(entity);
    }

    public async Task<ClaimTypeDto> UpdateClaimTypeAsync(long id, UpsertClaimTypeRequest request, CancellationToken cancellationToken = default)
    {
        await _claimTypeValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetRequiredAsync(() => _repository.GetClaimTypeByIdAsync(id, cancellationToken), $"Claim type '{id}' not found.");
        entity.ClaimTypeCode = request.ClaimTypeCode.Trim();
        entity.ClaimTypeName = request.ClaimTypeName.Trim();
        entity.ClaimTypeDescription = request.ClaimTypeDescription?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateClaimType(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToClaimTypeDto(entity);
    }

    public async Task DeleteClaimTypeAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequiredAsync(() => _repository.GetClaimTypeByIdAsync(id, cancellationToken), $"Claim type '{id}' not found.");
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateClaimType(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimStatusDto>> GetClaimStatusesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetClaimStatusesAsync(cancellationToken);
        return items.Select(ToClaimStatusDto).ToList();
    }

    public async Task<ClaimStatusDto?> GetClaimStatusByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetClaimStatusByIdAsync(id, cancellationToken);
        return item is null ? null : ToClaimStatusDto(item);
    }

    public async Task<ClaimStatusDto> CreateClaimStatusAsync(UpsertClaimStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _claimStatusValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = new ClaimStatusMaster
        {
            StatusCode = request.StatusCode.Trim(),
            StatusName = request.StatusName.Trim(),
            SequenceNo = request.SequenceNo,
            IsTerminalStatus = request.IsTerminalStatus,
            CreatedBy = "api"
        };
        await _repository.AddClaimStatusAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToClaimStatusDto(entity);
    }

    public async Task<ClaimStatusDto> UpdateClaimStatusAsync(long id, UpsertClaimStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _claimStatusValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetRequiredAsync(() => _repository.GetClaimStatusByIdAsync(id, cancellationToken), $"Claim status '{id}' not found.");
        entity.StatusCode = request.StatusCode.Trim();
        entity.StatusName = request.StatusName.Trim();
        entity.SequenceNo = request.SequenceNo;
        entity.IsTerminalStatus = request.IsTerminalStatus;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateClaimStatus(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToClaimStatusDto(entity);
    }

    public async Task DeleteClaimStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequiredAsync(() => _repository.GetClaimStatusByIdAsync(id, cancellationToken), $"Claim status '{id}' not found.");
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateClaimStatus(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InsuranceProductDto>> GetInsuranceProductsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetInsuranceProductsAsync(cancellationToken);
        return items.Select(ToInsuranceProductDto).ToList();
    }

    public async Task<InsuranceProductDto?> GetInsuranceProductByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetInsuranceProductByIdAsync(id, cancellationToken);
        return item is null ? null : ToInsuranceProductDto(item);
    }

    public async Task<InsuranceProductDto> CreateInsuranceProductAsync(UpsertInsuranceProductRequest request, CancellationToken cancellationToken = default)
    {
        await _insuranceProductValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = new InsuranceProduct
        {
            PolicyTypeCode = request.PolicyTypeCode.Trim(),
            PolicyTypeName = request.PolicyTypeName.Trim(),
            PolicyTypeDescription = request.PolicyTypeDescription?.Trim(),
            CreatedBy = "api"
        };
        await _repository.AddInsuranceProductAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToInsuranceProductDto(entity);
    }

    public async Task<InsuranceProductDto> UpdateInsuranceProductAsync(long id, UpsertInsuranceProductRequest request, CancellationToken cancellationToken = default)
    {
        await _insuranceProductValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetRequiredAsync(() => _repository.GetInsuranceProductByIdAsync(id, cancellationToken), $"Insurance product '{id}' not found.");
        entity.PolicyTypeCode = request.PolicyTypeCode.Trim();
        entity.PolicyTypeName = request.PolicyTypeName.Trim();
        entity.PolicyTypeDescription = request.PolicyTypeDescription?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateInsuranceProduct(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToInsuranceProductDto(entity);
    }

    public async Task DeleteInsuranceProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequiredAsync(() => _repository.GetInsuranceProductByIdAsync(id, cancellationToken), $"Insurance product '{id}' not found.");
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateInsuranceProduct(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FraudRuleDto>> GetFraudRulesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetFraudRulesAsync(cancellationToken);
        return items.Select(ToFraudRuleDto).ToList();
    }

    public async Task<FraudRuleDto?> GetFraudRuleByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetFraudRuleByIdAsync(id, cancellationToken);
        return item is null ? null : ToFraudRuleDto(item);
    }

    public async Task<FraudRuleDto> CreateFraudRuleAsync(UpsertFraudRuleRequest request, CancellationToken cancellationToken = default)
    {
        await _fraudRuleValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = new FraudRuleMaster
        {
            RuleCode = request.RuleCode.Trim(),
            RuleName = request.RuleName.Trim(),
            RuleWeight = request.RuleWeight,
            RuleDefinition = request.RuleDefinition?.Trim(),
            CreatedBy = "api"
        };
        await _repository.AddFraudRuleAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToFraudRuleDto(entity);
    }

    public async Task<FraudRuleDto> UpdateFraudRuleAsync(long id, UpsertFraudRuleRequest request, CancellationToken cancellationToken = default)
    {
        await _fraudRuleValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetRequiredAsync(() => _repository.GetFraudRuleByIdAsync(id, cancellationToken), $"Fraud rule '{id}' not found.");
        entity.RuleCode = request.RuleCode.Trim();
        entity.RuleName = request.RuleName.Trim();
        entity.RuleWeight = request.RuleWeight;
        entity.RuleDefinition = request.RuleDefinition?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateFraudRule(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToFraudRuleDto(entity);
    }

    public async Task DeleteFraudRuleAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequiredAsync(() => _repository.GetFraudRuleByIdAsync(id, cancellationToken), $"Fraud rule '{id}' not found.");
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateFraudRule(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowStageDto>> GetWorkflowStagesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetWorkflowStagesAsync(cancellationToken);
        return items.Select(ToWorkflowStageDto).ToList();
    }

    public async Task<WorkflowStageDto?> GetWorkflowStageByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetWorkflowStageByIdAsync(id, cancellationToken);
        return item is null ? null : ToWorkflowStageDto(item);
    }

    public async Task<WorkflowStageDto> CreateWorkflowStageAsync(UpsertWorkflowStageRequest request, CancellationToken cancellationToken = default)
    {
        await _workflowStageValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = new WorkflowStageMaster
        {
            WorkflowDefinitionId = request.WorkflowDefinitionId,
            StageCode = request.StageCode.Trim(),
            StageName = request.StageName.Trim(),
            StageSequence = request.StageSequence,
            SlaInHours = request.SlaInHours,
            CreatedBy = "api"
        };
        await _repository.AddWorkflowStageAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToWorkflowStageDto(entity);
    }

    public async Task<WorkflowStageDto> UpdateWorkflowStageAsync(long id, UpsertWorkflowStageRequest request, CancellationToken cancellationToken = default)
    {
        await _workflowStageValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetRequiredAsync(() => _repository.GetWorkflowStageByIdAsync(id, cancellationToken), $"Workflow stage '{id}' not found.");
        entity.WorkflowDefinitionId = request.WorkflowDefinitionId;
        entity.StageCode = request.StageCode.Trim();
        entity.StageName = request.StageName.Trim();
        entity.StageSequence = request.StageSequence;
        entity.SlaInHours = request.SlaInHours;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateWorkflowStage(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToWorkflowStageDto(entity);
    }

    public async Task DeleteWorkflowStageAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequiredAsync(() => _repository.GetWorkflowStageByIdAsync(id, cancellationToken), $"Workflow stage '{id}' not found.");
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        _repository.UpdateWorkflowStage(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ClaimTypeDto ToClaimTypeDto(ClaimTypeMaster entity) => new()
    {
        ClaimTypeId = entity.ClaimTypeId,
        ClaimTypeCode = entity.ClaimTypeCode,
        ClaimTypeName = entity.ClaimTypeName,
        ClaimTypeDescription = entity.ClaimTypeDescription
    };

    private static ClaimStatusDto ToClaimStatusDto(ClaimStatusMaster entity) => new()
    {
        ClaimStatusId = entity.ClaimStatusId,
        StatusCode = entity.StatusCode,
        StatusName = entity.StatusName,
        SequenceNo = entity.SequenceNo,
        IsTerminalStatus = entity.IsTerminalStatus
    };

    private static InsuranceProductDto ToInsuranceProductDto(InsuranceProduct entity) => new()
    {
        PolicyTypeId = entity.PolicyTypeId,
        PolicyTypeCode = entity.PolicyTypeCode,
        PolicyTypeName = entity.PolicyTypeName,
        PolicyTypeDescription = entity.PolicyTypeDescription
    };

    private static FraudRuleDto ToFraudRuleDto(FraudRuleMaster entity) => new()
    {
        FraudRuleId = entity.FraudRuleId,
        RuleCode = entity.RuleCode,
        RuleName = entity.RuleName,
        RuleWeight = entity.RuleWeight,
        RuleDefinition = entity.RuleDefinition
    };

    private static WorkflowStageDto ToWorkflowStageDto(WorkflowStageMaster entity) => new()
    {
        WorkflowStageId = entity.WorkflowStageId,
        WorkflowDefinitionId = entity.WorkflowDefinitionId,
        StageCode = entity.StageCode,
        StageName = entity.StageName,
        StageSequence = entity.StageSequence,
        SlaInHours = entity.SlaInHours
    };

    private static async Task<T> GetRequiredAsync<T>(Func<Task<T?>> resolver, string notFoundMessage) where T : class
    {
        var entity = await resolver();
        return entity ?? throw new KeyNotFoundException(notFoundMessage);
    }
}

