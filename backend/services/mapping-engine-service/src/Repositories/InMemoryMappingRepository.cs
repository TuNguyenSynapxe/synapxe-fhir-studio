using MappingEngineService.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MappingEngineService.Repositories;

public class InMemoryMappingRepository : IMappingRepository
{
    private readonly ConcurrentDictionary<string, MappingDefinition> _mappings = new();
    private readonly ILogger<InMemoryMappingRepository> _logger;

    public InMemoryMappingRepository(ILogger<InMemoryMappingRepository> logger)
    {
        _logger = logger;
        SeedData();
    }

    private void SeedData()
    {
        // Seed with sample Patient Demographics mapping
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            ProjectId = "project-001",
            Name = "Patient Demographics Mapping",
            Description = "Maps patient demographic data to FHIR Patient resource",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Version = 1,
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    Id = "item-001",
                    SourcePath = "patient.firstName",
                    TargetPath = "name[0].given[0]",
                    IsRequired = true
                },
                new MappingItem
                {
                    Id = "item-002",
                    SourcePath = "patient.lastName",
                    TargetPath = "name[0].family",
                    TransformationExpression = "UPPERCASE",
                    IsRequired = true
                },
                new MappingItem
                {
                    Id = "item-003",
                    SourcePath = "patient.birthDate",
                    TargetPath = "birthDate",
                    IsRequired = false
                },
                new MappingItem
                {
                    Id = "item-004",
                    SourcePath = "patient.gender",
                    TargetPath = "gender",
                    TransformationExpression = "LOWERCASE",
                    IsRequired = false
                }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _mappings.TryAdd(mapping.Id, mapping);
        _logger.LogInformation("Seeded {Count} mappings", _mappings.Count);
    }

    public Task<MappingDefinition?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving mapping by id: {MappingId}", id);
        _mappings.TryGetValue(id, out var mapping);
        return Task.FromResult(mapping);
    }
}
