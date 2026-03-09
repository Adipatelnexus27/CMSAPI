using CMSAPI.Application.DTOs.Claims;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class CreateClaimRequestDtoValidator : AbstractValidator<CreateClaimRequestDto>
{
    public CreateClaimRequestDtoValidator()
    {
        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .MaximumLength(40);

        RuleFor(x => x.ClaimantName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.IncidentDateUtc)
            .Must(incidentDateUtc => incidentDateUtc <= DateTime.UtcNow)
            .WithMessage("Incident date cannot be in the future.");

        RuleFor(x => x.ClaimedAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ReservedAmount)
            .GreaterThanOrEqualTo(0);
    }
}
