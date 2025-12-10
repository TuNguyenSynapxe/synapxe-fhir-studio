using MappingService.DTOs;
using MappingService.Models;
using MappingService.Repositories;

namespace MappingService.Services;

public class MappingService : IMappingService
{
    private readonly IMappingRepository _repository;
    private readonly ILogger<MappingService> _logger;

    public MappingService(IMappingRepository repository, ILogger<MappingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<MappingResponse>> GetAllMappingsAsync(string? projectId = null)
    {
        _logger.LogInformation("Getting all mappings with projectId: {ProjectId}", projectId);
        
        var mappings = await _repository.GetAllAsync(projectId);
        return mappings.Select(MappingResponse.FromMappingDefinition);
    }

    public async Task<MappingResponse?> GetMappingByIdAsync(string id)
    {
        _logger.LogInformation("Getting mapping by id: {MappingId}", id);
        
        var mapping = await _repository.GetByIdAsync(id);
        return mapping != null ? MappingResponse.FromMappingDefinition(mapping) : null;
    }

    public async Task<MappingResponse> CreateMappingAsync(MappingRequest request, string createdBy = "system")
    {
        _logger.LogInformation("Creating new mapping: {MappingName}", request.Name);
        
        var mapping = new MappingDefinition
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            SourceSchemaId = request.SourceSchemaId,
            TemplateId = request.TemplateId,
            FhirVersion = request.FhirVersion,
            Version = 1,
            Status = MappingStatus.Draft,
            Items = request.Items.Select(dto => new MappingItem
            {
                Id = dto.Id ?? Guid.NewGuid().ToString(),
                SourcePath = dto.SourcePath,
                TargetPath = dto.TargetPath,
                TransformationExpression = dto.TransformationExpression,
                Notes = dto.Notes,
                IsRequired = dto.IsRequired
            }).ToList(),
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
        
        var created = await _repository.CreateAsync(mapping);
        return MappingResponse.FromMappingDefinition(created);
    }

    public async Task<MappingResponse?> UpdateMappingAsync(string id, MappingRequest request, string updatedBy = "system")
    {
        _logger.LogInformation("Updating mapping: {MappingId}", id);
        
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Mapping not found: {MappingId}", id);
            return null;
        }
        
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return null;
        }
        
        // Cannot update Active mappings directly - must create new version or deprecate first
        if (existing.Status == MappingStatus.Active)
        {
            _logger.LogWarning("Cannot update Active mapping: {MappingId}. Publish a new version instead.", id);
            throw new InvalidOperationException("Cannot update Active mapping. Create a new draft or deprecate the active mapping first.");
        }
        
        var mapping = new MappingDefinition
        {
            Name = request.Name,
            Description = request.Description,
            SourceSchemaId = request.SourceSchemaId,
            TemplateId = request.TemplateId,
            FhirVersion = request.FhirVersion,
            Items = request.Items.Select(dto => new MappingItem
            {
                Id = dto.Id ?? Guid.NewGuid().ToString(),
                SourcePath = dto.SourcePath,
                TargetPath = dto.TargetPath,
                TransformationExpression = dto.TransformationExpression,
                Notes = dto.Notes,
                IsRequired = dto.IsRequired
            }).ToList(),
            UpdatedBy = updatedBy
        };
        
        var updated = await _repository.UpdateAsync(id, mapping);
        return updated != null ? MappingResponse.FromMappingDefinition(updated) : null;
    }

    public async Task<MappingResponse?> PublishMappingAsync(string id, string updatedBy = "system")
    {
        _logger.LogInformation("Publishing mapping: {MappingId}", id);
        
        var mapping = await _repository.GetByIdAsync(id);
        if (mapping == null)
        {
            _logger.LogWarning("Mapping not found: {MappingId}", id);
            return null;
        }
        
        if (mapping.Status == MappingStatus.Active)
        {
            _logger.LogWarning("Mapping is already Active: {MappingId}", id);
            throw new InvalidOperationException("Mapping is already Active.");
        }
        
        if (mapping.Status == MappingStatus.Deprecated)
        {
            _logger.LogWarning("Cannot publish Deprecated mapping: {MappingId}", id);
            throw new InvalidOperationException("Cannot publish a Deprecated mapping.");
        }
        
        // Check for existing Active mapping with same combination
        var existingActive = await _repository.GetActiveMappingAsync(
            mapping.ProjectId,
            mapping.SourceSchemaId,
            mapping.TemplateId,
            mapping.FhirVersion);
        
        if (existingActive != null)
        {
            _logger.LogInformation("Deprecating existing Active mapping: {ExistingMappingId}", existingActive.Id);
            // Deprecate the existing active mapping
            existingActive.Status = MappingStatus.Deprecated;
            existingActive.UpdatedAt = DateTime.UtcNow;
            existingActive.UpdatedBy = updatedBy;
            await _repository.UpdateAsync(existingActive.Id, existingActive);
        }
        
        // Publish the new mapping
        mapping.Status = MappingStatus.Active;
        mapping.Version = existingActive != null ? existingActive.Version + 1 : mapping.Version;
        mapping.UpdatedAt = DateTime.UtcNow;
        mapping.UpdatedBy = updatedBy;
        
        var published = await _repository.UpdateAsync(id, mapping);
        
        _logger.LogInformation("Published mapping {MappingId} as version {Version}", id, mapping.Version);
        
        return published != null ? MappingResponse.FromMappingDefinition(published) : null;
    }

    public async Task<bool> DeleteMappingAsync(string id)
    {
        _logger.LogInformation("Deleting mapping: {MappingId}", id);
        
        var deleted = await _repository.DeleteAsync(id);
        
        if (!deleted)
        {
            _logger.LogWarning("Mapping not found for deletion: {MappingId}", id);
        }
        
        return deleted;
    }
}
