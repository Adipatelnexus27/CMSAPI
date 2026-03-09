using CMSAPI.Application.DTOs.Claims;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class UpdateClaimStatusRequestDtoValidator : AbstractValidator<UpdateClaimStatusRequestDto>
{
    public UpdateClaimStatusRequestDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}

