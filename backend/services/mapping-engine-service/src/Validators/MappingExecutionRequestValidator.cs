using FluentValidation;
using MappingEngineService.DTOs;

namespace MappingEngineService.Validators;

public class MappingExecutionRequestValidator : AbstractValidator<MappingExecutionRequest>
{
    public MappingExecutionRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required");

        RuleFor(x => x.MappingId)
            .NotEmpty()
            .WithMessage("MappingId is required");

        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required");

        RuleFor(x => x.SourcePayload)
            .NotNull()
            .WithMessage("SourcePayload is required");
    }
}
