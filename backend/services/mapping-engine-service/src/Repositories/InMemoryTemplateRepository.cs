using MappingEngineService.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MappingEngineService.Repositories;

public class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly ConcurrentDictionary<string, Template> _templates = new();
    private readonly ILogger<InMemoryTemplateRepository> _logger;

    public InMemoryTemplateRepository(ILogger<InMemoryTemplateRepository> logger)
    {
        _logger = logger;
        SeedData();
    }

    private void SeedData()
    {
        // Seed with sample FHIR Patient template
        var template = new Template
        {
            Id = "template-001",
            ProjectId = "project-001",
            Name = "FHIR R4 Patient Template",
            Description = "Basic FHIR R4 Patient resource template",
            FhirVersion = "R4",
            FhirResourceType = "Patient",
            TemplateContent = @"{
  ""resourceType"": ""Patient"",
  ""id"": ""example"",
  ""meta"": {
    ""profile"": [""http://hl7.org/fhir/StructureDefinition/Patient""]
  },
  ""text"": {
    ""status"": ""generated"",
    ""div"": ""<div xmlns=\""http://www.w3.org/1999/xhtml\"">Patient Demographics</div>""
  },
  ""identifier"": [],
  ""active"": true,
  ""name"": [],
  ""telecom"": [],
  ""gender"": """",
  ""birthDate"": """",
  ""address"": []
}",
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            UpdatedAt = DateTime.UtcNow.AddDays(-60)
        };

        _templates.TryAdd(template.Id, template);
        _logger.LogInformation("Seeded {Count} templates", _templates.Count);
    }

    public Task<Template?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving template by id: {TemplateId}", id);
        _templates.TryGetValue(id, out var template);
        return Task.FromResult(template);
    }
}
