using TemplateService.Models;
using TemplateService.Repositories;

namespace TemplateService.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(ITemplateRepository repository, ILogger<TemplateService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<TemplateResponse>> GetAllTemplatesAsync(string? fhirVersion = null)
    {
        _logger.LogInformation("Getting all templates with fhirVersion: {FhirVersion}", fhirVersion);
        
        var templates = await _repository.GetAllAsync(fhirVersion);
        return templates.Select(TemplateResponse.FromTemplate);
    }

    public async Task<TemplateResponse?> GetTemplateByIdAsync(string id)
    {
        _logger.LogInformation("Getting template by id: {TemplateId}", id);
        
        var template = await _repository.GetByIdAsync(id);
        return template != null ? TemplateResponse.FromTemplate(template) : null;
    }

    public async Task<TemplateResponse> CreateTemplateAsync(TemplateRequest request)
    {
        _logger.LogInformation("Creating new template: {TemplateName}", request.Name);
        
        var template = new Template
        {
            Name = request.Name,
            ResourceType = request.ResourceType,
            FhirVersion = request.FhirVersion,
            TemplateContent = request.TemplateContent
        };
        
        var created = await _repository.CreateAsync(template);
        return TemplateResponse.FromTemplate(created);
    }

    public async Task<TemplateResponse?> UpdateTemplateAsync(string id, TemplateRequest request)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);
        
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Template not found: {TemplateId}", id);
            return null;
        }
        
        var template = new Template
        {
            Name = request.Name,
            ResourceType = request.ResourceType,
            FhirVersion = request.FhirVersion,
            TemplateContent = request.TemplateContent
        };
        
        var updated = await _repository.UpdateAsync(id, template);
        return updated != null ? TemplateResponse.FromTemplate(updated) : null;
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        
        var deleted = await _repository.DeleteAsync(id);
        
        if (!deleted)
        {
            _logger.LogWarning("Template not found for deletion: {TemplateId}", id);
        }
        
        return deleted;
    }
}
