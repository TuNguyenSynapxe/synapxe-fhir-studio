using TemplateService.Models;

namespace TemplateService.Services;

public interface ITemplateService
{
    Task<IEnumerable<TemplateResponse>> GetAllTemplatesAsync(string? fhirVersion = null);
    Task<TemplateResponse?> GetTemplateByIdAsync(string id);
    Task<TemplateResponse> CreateTemplateAsync(TemplateRequest request);
    Task<TemplateResponse?> UpdateTemplateAsync(string id, TemplateRequest request);
    Task<bool> DeleteTemplateAsync(string id);
}
