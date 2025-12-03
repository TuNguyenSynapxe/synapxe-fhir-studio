namespace SchemaParserService.Models;

/// <summary>
/// Represents a hierarchical node in a schema tree structure.
/// Used for parsing legacy specification CSVs with column-based indentation.
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

    /// <summary>
    /// Flattens the tree structure into a list of SchemaFields for backward compatibility
    /// </summary>
    public List<SchemaField> ToSchemaFields()
    {
        var fields = new List<SchemaField>();
        FlattenNode(this, fields, string.Empty);
        return fields;
    }

    private void FlattenNode(SchemaNode node, List<SchemaField> fields, string parentPath)
    {
        var currentPath = string.IsNullOrEmpty(parentPath) ? node.Name : $"{parentPath}.{node.Name}";

        var field = new SchemaField
        {
            Name = currentPath,
            DataType = node.DataType ?? "string",
            IsRequired = ParseCardinality(node.Cardinality, out bool isArray),
            IsArray = isArray,
            Description = node.Definition,
            MaxLength = ExtractMaxLength(node.DataType)
        };

        fields.Add(field);

        foreach (var child in node.Children)
        {
            FlattenNode(child, fields, currentPath);
        }
    }

    private bool ParseCardinality(string? cardinality, out bool isArray)
    {
        isArray = false;
        if (string.IsNullOrEmpty(cardinality))
            return false;

        // Check if it's an array (contains "…" or multiple occurrences like "0 … *" or "1 … 2")
        if (cardinality.Contains("…") || cardinality.Contains("*"))
        {
            isArray = true;
        }

        // Required if starts with "1" or contains "Mandatory"
        return cardinality.StartsWith("1") || cardinality.Contains("Mandatory", StringComparison.OrdinalIgnoreCase);
    }

    private int? ExtractMaxLength(string? dataType)
    {
        if (string.IsNullOrEmpty(dataType))
            return null;

        // Extract number from "String (50)" or "Long (15)"
        var match = System.Text.RegularExpressions.Regex.Match(dataType, @"\((\d+)\)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int length))
        {
            return length;
        }

        return null;
    }
}
