using CMSAPI.Application.DTOs.Claims;
using FluentValidation;

namespace CMSAPI.Application.Validators;

public sealed class UploadClaimDocumentRequestDtoValidator : AbstractValidator<UploadClaimDocumentRequestDto>
{
    public UploadClaimDocumentRequestDtoValidator()
    {
        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260);

        RuleFor(x => x.Content)
            .NotNull()
            .Must(content => content is { Length: > 0 })
            .WithMessage("A document file is required.");
    }
}

public sealed class LinkRelatedClaimRequestDtoValidator : AbstractValidator<LinkRelatedClaimRequestDto>
{
    public LinkRelatedClaimRequestDtoValidator()
    {
        RuleFor(x => x.RelatedClaimId)
            .GreaterThan(0);
    }
}
