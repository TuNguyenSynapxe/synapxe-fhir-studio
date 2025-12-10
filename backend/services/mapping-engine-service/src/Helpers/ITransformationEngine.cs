namespace MappingEngineService.Helpers;

public interface ITransformationEngine
{
    /// <summary>
    /// Applies transformation expression to input value
    /// Phase 1: Simple string-based transformations
    /// </summary>
    string? Transform(string? inputValue, string? transformationExpression);
}
