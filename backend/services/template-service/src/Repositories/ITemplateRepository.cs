using TemplateService.Models;

namespace TemplateService.Repositories;

public interface ITemplateRepository
{
    Task<IEnumerable<Template>> GetAllAsync(string? fhirVersion = null);
    Task<Template?> GetByIdAsync(string id);
    Task<Template> CreateAsync(Template template);
    Task<Template?> UpdateAsync(string id, Template template);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
