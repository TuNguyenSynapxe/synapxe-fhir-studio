using FluentValidation;
using SchemaParserService.Models;

namespace SchemaParserService.Validators;

public class ParseSchemaRequestValidator : AbstractValidator<ParseSchemaRequest>
{
    private static readonly string[] ValidSourceTypes = { "csv", "xml", "json", "xsd" };

    public ParseSchemaRequestValidator()
    {
        RuleFor(x => x.SourceType)
            .NotEmpty()
            .WithMessage("SourceType is required")
            .Must(st => ValidSourceTypes.Contains(st.ToLowerInvariant()))
            .WithMessage($"SourceType must be one of: {string.Join(", ", ValidSourceTypes)}");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required");
    }
}
