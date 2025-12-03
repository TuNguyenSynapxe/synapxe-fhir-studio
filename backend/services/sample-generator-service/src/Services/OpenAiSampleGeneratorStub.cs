namespace SampleGeneratorService.Services;

/// <summary>
/// Stub implementation of IOpenAiSampleGenerator.
/// Always returns null to fallback to deterministic generation.
/// </summary>
public class OpenAiSampleGeneratorStub : IOpenAiSampleGenerator
{
    private readonly ILogger<OpenAiSampleGeneratorStub> _logger;

    public OpenAiSampleGeneratorStub(ILogger<OpenAiSampleGeneratorStub> logger)
    {
        _logger = logger;
    }

    public string? GenerateValue(string fieldName, string? dataType, string? description)
    {
        _logger.LogDebug("AI generation not implemented, falling back to deterministic generation for field: {FieldName}", fieldName);
        return null; // Always fallback to deterministic
    }
}
