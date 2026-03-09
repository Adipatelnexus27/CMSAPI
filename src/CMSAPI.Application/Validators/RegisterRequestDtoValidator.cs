using CMSAPI.Application.DTOs.Auth;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can contain letters, numbers, dot, underscore and hyphen.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.RoleCode)
            .NotEmpty()
            .MaximumLength(50);
    }
}

