namespace SampleGeneratorService.Models;

/// <summary>
/// Represents a hierarchical node in a schema tree structure.
/// </summary>
public class SchemaNode
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? DataType { get; set; }
    public string? Cardinality { get; set; }
    public string? Definition { get; set; }
    public string? SampleValue { get; set; }
    public string? FhirMapping { get; set; }
    public string? Significance { get; set; }
    public List<SchemaNode> Children { get; set; } = new();
}
