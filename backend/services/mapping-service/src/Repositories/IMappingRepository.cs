using MappingService.Models;

namespace MappingService.Repositories;

public interface IMappingRepository
{
    Task<IEnumerable<MappingDefinition>> GetAllAsync(string? projectId = null);
    Task<MappingDefinition?> GetByIdAsync(string id);
    Task<MappingDefinition> CreateAsync(MappingDefinition mapping);
    Task<MappingDefinition?> UpdateAsync(string id, MappingDefinition mapping);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<MappingDefinition?> GetActiveMappingAsync(string projectId, string sourceSchemaId, string templateId, string fhirVersion);
    Task<IEnumerable<MappingDefinition>> GetActiveMappingsAsync(string projectId, string sourceSchemaId, string templateId, string fhirVersion);
}
