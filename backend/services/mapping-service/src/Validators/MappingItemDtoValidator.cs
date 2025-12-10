using FluentValidation;
using MappingService.DTOs;

namespace MappingService.Validators;

public class MappingItemDtoValidator : AbstractValidator<MappingItemDto>
{
    public MappingItemDtoValidator()
    {
        RuleFor(x => x.SourcePath)
            .NotEmpty()
            .WithMessage("SourcePath is required")
            .MaximumLength(500)
            .WithMessage("SourcePath must not exceed 500 characters");

        RuleFor(x => x.TargetPath)
            .NotEmpty()
            .WithMessage("TargetPath is required")
            .MaximumLength(500)
            .WithMessage("TargetPath must not exceed 500 characters");

        RuleFor(x => x.TransformationExpression)
            .MaximumLength(1000)
            .WithMessage("TransformationExpression must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TransformationExpression));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
