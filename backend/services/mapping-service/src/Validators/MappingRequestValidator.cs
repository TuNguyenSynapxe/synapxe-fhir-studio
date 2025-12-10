using FluentValidation;
using MappingService.DTOs;

namespace MappingService.Validators;

public class MappingRequestValidator : AbstractValidator<MappingRequest>
{
    public MappingRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.SourceSchemaId)
            .NotEmpty()
            .WithMessage("SourceSchemaId is required");

        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required");

        RuleFor(x => x.FhirVersion)
            .NotEmpty()
            .WithMessage("FhirVersion is required")
            .Must(version => version == "R4" || version == "R5")
            .WithMessage("FhirVersion must be either 'R4' or 'R5'");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items cannot be null");

        RuleForEach(x => x.Items)
            .SetValidator(new MappingItemDtoValidator());
    }
}
