using MappingEngineService.Models;

namespace MappingEngineService.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(string id);
}
