namespace SampleGeneratorService.Models;

public class SchemaDefinition
{
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public List<SchemaField> Fields { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SchemaField
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsArray { get; set; }
    public string? Description { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public List<SchemaField>? NestedFields { get; set; }
}
