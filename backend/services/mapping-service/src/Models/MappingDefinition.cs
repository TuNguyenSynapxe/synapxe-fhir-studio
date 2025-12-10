namespace MappingService.Models;

public class MappingDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSchemaId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = "R4";
    public int Version { get; set; } = 1;
    public MappingStatus Status { get; set; } = MappingStatus.Draft;
    public List<MappingItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
