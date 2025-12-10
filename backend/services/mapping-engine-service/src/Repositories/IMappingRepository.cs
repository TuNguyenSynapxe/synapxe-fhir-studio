using MappingEngineService.Models;

namespace MappingEngineService.Repositories;

public interface IMappingRepository
{
    Task<MappingDefinition?> GetByIdAsync(string id);
}
