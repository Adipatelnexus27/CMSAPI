using CMSAPI.Application.DTOs.Policies;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class CreatePolicyRequestDtoValidator : AbstractValidator<CreatePolicyRequestDto>
{
    public CreatePolicyRequestDtoValidator()
    {
        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PolicyTypeId)
            .GreaterThan(0);

        RuleFor(x => x.InsuredName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PolicyStartDate)
            .LessThanOrEqualTo(x => x.PolicyEndDate)
            .WithMessage("Policy start date must be less than or equal to policy end date.");

        RuleFor(x => x.SumInsured)
            .GreaterThan(0);

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0);

        RuleFor(x => x.PolicyStatus)
            .NotEmpty()
            .MaximumLength(50);

        RuleForEach(x => x.Coverages).SetValidator(new CreatePolicyCoverageRequestDtoValidator());
    }
}

public sealed class CreatePolicyCoverageRequestDtoValidator : AbstractValidator<CreatePolicyCoverageRequestDto>
{
    public CreatePolicyCoverageRequestDtoValidator()
    {
        RuleFor(x => x.CoverageTypeId)
            .GreaterThan(0);

        RuleFor(x => x.CoverageLimit)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DeductibleAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DeductibleAmount.HasValue);

        RuleFor(x => x.EffectiveFrom)
            .LessThanOrEqualTo(x => x.EffectiveTo)
            .WithMessage("Coverage effective-from date must be less than or equal to effective-to date.");
    }
}

