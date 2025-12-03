using FluentValidation;
using SampleGeneratorService.Models;

namespace SampleGeneratorService.Validators;

public class SampleGenerationRequestValidator : AbstractValidator<SampleGenerationRequest>
{
    public SampleGenerationRequestValidator()
    {
        RuleFor(x => x.RecordCount)
            .GreaterThan(0)
            .WithMessage("RecordCount must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("RecordCount must not exceed 100");

        RuleFor(x => x)
            .Must(x => x.SchemaDefinition != null || x.HierarchicalSchema != null)
            .WithMessage("Either SchemaDefinition or HierarchicalSchema must be provided");

        When(x => x.SchemaDefinition != null, () =>
        {
            RuleFor(x => x.SchemaDefinition!.Fields)
                .NotEmpty()
                .WithMessage("SchemaDefinition must contain at least one field");
        });

        When(x => x.HierarchicalSchema != null, () =>
        {
            RuleFor(x => x.HierarchicalSchema)
                .NotEmpty()
                .WithMessage("HierarchicalSchema must contain at least one node");
        });
    }
}
