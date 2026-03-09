using CMSAPI.Application.DTOs.Claims;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class CreateClaimRequestDtoValidator : AbstractValidator<CreateClaimRequestDto>
{
    public CreateClaimRequestDtoValidator()
    {
        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ClaimTypeId)
            .GreaterThan(0);

        RuleFor(x => x.LossDateUtc)
            .Must(lossDateUtc => lossDateUtc <= DateTime.UtcNow)
            .WithMessage("Loss date cannot be in the future.");

        RuleFor(x => x.IncidentDescription)
            .MaximumLength(2000);

        RuleFor(x => x.LocationOfLoss)
            .MaximumLength(500);

        RuleFor(x => x.EstimatedLossAmount)
            .GreaterThan(0);

        RuleFor(x => x.ClaimantName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ClaimantContactNo)
            .MaximumLength(30);

        RuleFor(x => x.ClaimantEmail)
            .MaximumLength(256)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ClaimantEmail));

        RuleFor(x => x.ClaimantAddressLine)
            .MaximumLength(300);

        RuleFor(x => x.ClaimantCity)
            .MaximumLength(100);

        RuleFor(x => x.ClaimantState)
            .MaximumLength(100);

        RuleFor(x => x.ClaimantPostalCode)
            .MaximumLength(20);

        RuleForEach(x => x.RelatedClaimIds)
            .GreaterThan(0);

        RuleFor(x => x.RelatedClaimIds)
            .NotNull()
            .WithMessage("Related claims must be an array.");

        RuleFor(x => x.RelatedClaimIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .When(x => x.RelatedClaimIds is { Count: > 0 })
            .WithMessage("Related claims must be unique.");
    }
}
