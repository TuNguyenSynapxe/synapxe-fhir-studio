using MappingService.Models;

namespace MappingService.DTOs;

public class MappingResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSchemaId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<MappingItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    public static MappingResponse FromMappingDefinition(MappingDefinition mapping)
    {
        return new MappingResponse
        {
            Id = mapping.Id,
            ProjectId = mapping.ProjectId,
            Name = mapping.Name,
            Description = mapping.Description,
            SourceSchemaId = mapping.SourceSchemaId,
            TemplateId = mapping.TemplateId,
            FhirVersion = mapping.FhirVersion,
            Version = mapping.Version,
            Status = mapping.Status.ToString(),
            Items = mapping.Items.Select(item => new MappingItemDto
            {
                Id = item.Id,
                SourcePath = item.SourcePath,
                TargetPath = item.TargetPath,
                TransformationExpression = item.TransformationExpression,
                Notes = item.Notes,
                IsRequired = item.IsRequired
            }).ToList(),
            CreatedAt = mapping.CreatedAt,
            UpdatedAt = mapping.UpdatedAt,
            CreatedBy = mapping.CreatedBy,
            UpdatedBy = mapping.UpdatedBy
        };
    }
}
