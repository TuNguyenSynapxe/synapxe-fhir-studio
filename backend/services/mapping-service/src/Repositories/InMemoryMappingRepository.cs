using System.Collections.Concurrent;
using MappingService.Models;

namespace MappingService.Repositories;

/// <summary>
/// Phase 1 implementation: In-memory storage using ConcurrentDictionary
/// Phase 2 will replace with PostgreSQL implementation
/// </summary>
public class InMemoryMappingRepository : IMappingRepository
{
    private readonly ConcurrentDictionary<string, MappingDefinition> _mappings = new();
    private readonly ILogger<InMemoryMappingRepository> _logger;

    public InMemoryMappingRepository(ILogger<InMemoryMappingRepository> logger)
    {
        _logger = logger;
        SeedData();
    }

    public Task<IEnumerable<MappingDefinition>> GetAllAsync(string? projectId = null)
    {
        _logger.LogInformation("Retrieving all mappings with projectId filter: {ProjectId}", projectId);
        
        var mappings = _mappings.Values.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(projectId))
        {
            mappings = mappings.Where(m => m.ProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase));
        }
        
        return Task.FromResult<IEnumerable<MappingDefinition>>(mappings.OrderByDescending(m => m.CreatedAt).ToList());
    }

    public Task<MappingDefinition?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving mapping by id: {MappingId}", id);
        _mappings.TryGetValue(id, out var mapping);
        return Task.FromResult(mapping);
    }

    public Task<MappingDefinition> CreateAsync(MappingDefinition mapping)
    {
        _logger.LogInformation("Creating new mapping: {MappingName}", mapping.Name);
        
        mapping.Id = Guid.NewGuid().ToString();
        mapping.CreatedAt = DateTime.UtcNow;
        mapping.UpdatedAt = DateTime.UtcNow;
        
        _mappings[mapping.Id] = mapping;
        return Task.FromResult(mapping);
    }

    public Task<MappingDefinition?> UpdateAsync(string id, MappingDefinition mapping)
    {
        _logger.LogInformation("Updating mapping: {MappingId}", id);
        
        if (!_mappings.TryGetValue(id, out var existing))
        {
            return Task.FromResult<MappingDefinition?>(null);
        }
        
        existing.Name = mapping.Name;
        existing.Description = mapping.Description;
        existing.SourceSchemaId = mapping.SourceSchemaId;
        existing.TemplateId = mapping.TemplateId;
        existing.FhirVersion = mapping.FhirVersion;
        existing.Items = mapping.Items;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = mapping.UpdatedBy;
        
        _mappings[id] = existing;
        return Task.FromResult<MappingDefinition?>(existing);
    }

    public Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting mapping: {MappingId}", id);
        
        if (_mappings.TryGetValue(id, out var mapping))
        {
            // Soft delete: set status to Deprecated
            mapping.Status = MappingStatus.Deprecated;
            mapping.UpdatedAt = DateTime.UtcNow;
            _mappings[id] = mapping;
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_mappings.ContainsKey(id));
    }

    public Task<MappingDefinition?> GetActiveMappingAsync(string projectId, string sourceSchemaId, string templateId, string fhirVersion)
    {
        _logger.LogInformation("Getting active mapping for ProjectId: {ProjectId}, SourceSchemaId: {SourceSchemaId}, TemplateId: {TemplateId}, FhirVersion: {FhirVersion}",
            projectId, sourceSchemaId, templateId, fhirVersion);
        
        var activeMapping = _mappings.Values
            .FirstOrDefault(m => 
                m.ProjectId == projectId &&
                m.SourceSchemaId == sourceSchemaId &&
                m.TemplateId == templateId &&
                m.FhirVersion == fhirVersion &&
                m.Status == MappingStatus.Active);
        
        return Task.FromResult(activeMapping);
    }

    public Task<IEnumerable<MappingDefinition>> GetActiveMappingsAsync(string projectId, string sourceSchemaId, string templateId, string fhirVersion)
    {
        _logger.LogInformation("Getting all active mappings for ProjectId: {ProjectId}, SourceSchemaId: {SourceSchemaId}, TemplateId: {TemplateId}, FhirVersion: {FhirVersion}",
            projectId, sourceSchemaId, templateId, fhirVersion);
        
        var activeMappings = _mappings.Values
            .Where(m => 
                m.ProjectId == projectId &&
                m.SourceSchemaId == sourceSchemaId &&
                m.TemplateId == templateId &&
                m.FhirVersion == fhirVersion &&
                m.Status == MappingStatus.Active)
            .ToList();
        
        return Task.FromResult<IEnumerable<MappingDefinition>>(activeMappings);
    }

    private void SeedData()
    {
        var sampleMapping = new MappingDefinition
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
                    IsRequired = true,
                    Notes = "Patient's first name"
                },
                new MappingItem
                {
                    Id = "item-002",
                    SourcePath = "patient.lastName",
                    TargetPath = "name[0].family",
                    IsRequired = true,
                    Notes = "Patient's family name"
                },
                new MappingItem
                {
                    Id = "item-003",
                    SourcePath = "patient.dateOfBirth",
                    TargetPath = "birthDate",
                    TransformationExpression = "toFHIRDate($value)",
                    IsRequired = true
                }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        _mappings[sampleMapping.Id] = sampleMapping;
        
        _logger.LogInformation("Seeded {Count} mappings", _mappings.Count);
    }
}
