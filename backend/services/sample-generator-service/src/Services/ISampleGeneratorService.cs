using SampleGeneratorService.Models;

namespace SampleGeneratorService.Services;

public interface ISampleGeneratorService
{
    Task<List<Dictionary<string, object>>> GenerateSamplesAsync(SampleGenerationRequest request);
}
