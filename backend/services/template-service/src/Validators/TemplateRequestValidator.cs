using FluentValidation;
using TemplateService.Models;

namespace TemplateService.Validators;

public class TemplateRequestValidator : AbstractValidator<TemplateRequest>
{
    public TemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .WithMessage("ResourceType is required")
            .MaximumLength(100)
            .WithMessage("ResourceType must not exceed 100 characters");

        RuleFor(x => x.FhirVersion)
            .NotEmpty()
            .WithMessage("FhirVersion is required")
            .Must(version => version == "R4" || version == "R5")
            .WithMessage("FhirVersion must be either 'R4' or 'R5'. Phase 1 only supports R4.");

        RuleFor(x => x.TemplateContent)
            .NotNull()
            .WithMessage("TemplateContent is required");
    }
}
