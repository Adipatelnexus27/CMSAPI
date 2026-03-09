using CMSAPI.Application.DTOs.ClaimAssignment;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class TriageClaimRequestDtoValidator : AbstractValidator<TriageClaimRequestDto>
{
    public TriageClaimRequestDtoValidator()
    {
        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}

public sealed class AssignClaimRoleRequestDtoValidator : AbstractValidator<AssignClaimRoleRequestDto>
{
    public AssignClaimRoleRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.AssignmentReason)
            .MaximumLength(500);
    }
}
