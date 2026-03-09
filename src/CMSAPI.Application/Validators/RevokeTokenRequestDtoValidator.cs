using CMSAPI.Application.DTOs.Auth;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class RevokeTokenRequestDtoValidator : AbstractValidator<RevokeTokenRequestDto>
{
    public RevokeTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(500);
    }
}

