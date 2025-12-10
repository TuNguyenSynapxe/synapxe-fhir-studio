namespace MappingService.DTOs;

public class MappingRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSchemaId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = "R4";
    public List<MappingItemDto> Items { get; set; } = new();
}
