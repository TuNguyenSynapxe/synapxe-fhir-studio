using System.Collections.Concurrent;
using TemplateService.Models;

namespace TemplateService.Repositories;

/// <summary>
/// Phase 1 implementation: In-memory storage using ConcurrentDictionary
/// Phase 2 will replace with PostgreSQL implementation
/// </summary>
public class StubTemplateRepository : ITemplateRepository
{
    private readonly ConcurrentDictionary<string, Template> _templates = new();
    private readonly ILogger<StubTemplateRepository> _logger;

    public StubTemplateRepository(ILogger<StubTemplateRepository> logger)
    {
        _logger = logger;
        SeedData();
    }

    public Task<IEnumerable<Template>> GetAllAsync(string? fhirVersion = null)
    {
        _logger.LogInformation("Retrieving all templates with fhirVersion filter: {FhirVersion}", fhirVersion);
        
        var templates = _templates.Values.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(fhirVersion))
        {
            templates = templates.Where(t => t.FhirVersion.Equals(fhirVersion, StringComparison.OrdinalIgnoreCase));
        }
        
        return Task.FromResult<IEnumerable<Template>>(templates.OrderBy(t => t.Name).ToList());
    }

    public Task<Template?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving template by id: {TemplateId}", id);
        _templates.TryGetValue(id, out var template);
        return Task.FromResult(template);
    }

    public Task<Template> CreateAsync(Template template)
    {
        _logger.LogInformation("Creating new template: {TemplateName}", template.Name);
        
        template.Id = Guid.NewGuid().ToString();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        
        _templates[template.Id] = template;
        return Task.FromResult(template);
    }

    public Task<Template?> UpdateAsync(string id, Template template)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);
        
        if (!_templates.TryGetValue(id, out var existing))
        {
            return Task.FromResult<Template?>(null);
        }
        
        existing.Name = template.Name;
        existing.ResourceType = template.ResourceType;
        existing.FhirVersion = template.FhirVersion;
        existing.TemplateContent = template.TemplateContent;
        existing.UpdatedAt = DateTime.UtcNow;
        
        _templates[id] = existing;
        return Task.FromResult<Template?>(existing);
    }

    public Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        return Task.FromResult(_templates.TryRemove(id, out _));
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_templates.ContainsKey(id));
    }

    private void SeedData()
    {
        var patientTemplate = new Template
        {
            Id = "template-001",
            Name = "Basic Patient Template",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = Newtonsoft.Json.Linq.JObject.Parse(@"{
                ""resourceType"": ""Patient"",
                ""identifier"": [],
                ""name"": [],
                ""gender"": """",
                ""birthDate"": """"
            }"),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var observationTemplate = new Template
        {
            Id = "template-002",
            Name = "Basic Observation Template",
            ResourceType = "Observation",
            FhirVersion = "R4",
            TemplateContent = Newtonsoft.Json.Linq.JObject.Parse(@"{
                ""resourceType"": ""Observation"",
                ""status"": ""final"",
                ""code"": {},
                ""subject"": {},
                ""valueQuantity"": {}
            }"),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _templates[patientTemplate.Id] = patientTemplate;
        _templates[observationTemplate.Id] = observationTemplate;
        
        _logger.LogInformation("Seeded {Count} templates", _templates.Count);
    }
}
