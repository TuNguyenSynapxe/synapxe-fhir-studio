using MappingEngineService.DTOs;

namespace MappingEngineService.Services;

public interface IMappingEngineService
{
    Task<MappingExecutionResponse> ExecuteMappingAsync(MappingExecutionRequest request);
}
