using CMSAPI.Application.DTOs.SystemConfiguration;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class UpsertClaimTypeRequestValidator : AbstractValidator<UpsertClaimTypeRequest>
{
    public UpsertClaimTypeRequestValidator()
    {
        RuleFor(x => x.ClaimTypeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ClaimTypeName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ClaimTypeDescription).MaximumLength(500);
    }
}

public sealed class UpsertClaimStatusRequestValidator : AbstractValidator<UpsertClaimStatusRequest>
{
    public UpsertClaimStatusRequestValidator()
    {
        RuleFor(x => x.StatusCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StatusName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.SequenceNo).GreaterThanOrEqualTo(1);
    }
}

public sealed class UpsertInsuranceProductRequestValidator : AbstractValidator<UpsertInsuranceProductRequest>
{
    public UpsertInsuranceProductRequestValidator()
    {
        RuleFor(x => x.PolicyTypeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PolicyTypeName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.PolicyTypeDescription).MaximumLength(500);
    }
}

public sealed class UpsertFraudRuleRequestValidator : AbstractValidator<UpsertFraudRuleRequest>
{
    public UpsertFraudRuleRequestValidator()
    {
        RuleFor(x => x.RuleCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RuleName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RuleWeight).InclusiveBetween(0, 100);
        RuleFor(x => x.RuleDefinition).MaximumLength(2000);
    }
}

public sealed class UpsertWorkflowStageRequestValidator : AbstractValidator<UpsertWorkflowStageRequest>
{
    public UpsertWorkflowStageRequestValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).GreaterThan(0);
        RuleFor(x => x.StageCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StageName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.StageSequence).GreaterThanOrEqualTo(1);
        RuleFor(x => x.SlaInHours).GreaterThanOrEqualTo(0).When(x => x.SlaInHours.HasValue);
    }
}

