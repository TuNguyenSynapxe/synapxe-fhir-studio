namespace SampleGeneratorService.Models;

public class SampleGenerationRequest
{
    public string? SchemaId { get; set; }
    public int RecordCount { get; set; } = 1;
    public SchemaDefinition? SchemaDefinition { get; set; }
    public List<SchemaNode>? HierarchicalSchema { get; set; }
    public bool UseAi { get; set; } = false;
    public int? Seed { get; set; }
}
