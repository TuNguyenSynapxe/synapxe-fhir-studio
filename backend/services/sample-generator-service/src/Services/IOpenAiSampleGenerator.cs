namespace SampleGeneratorService.Services;

/// <summary>
/// Interface for AI-assisted sample data generation.
/// This is a stub for future OpenAI integration.
/// </summary>
public interface IOpenAiSampleGenerator
{
    string? GenerateValue(string fieldName, string? dataType, string? description);
}
