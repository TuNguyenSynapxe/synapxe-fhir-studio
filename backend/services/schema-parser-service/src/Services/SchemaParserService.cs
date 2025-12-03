using SchemaParserService.Models;
using System.Text.Json;

namespace SchemaParserService.Services;

public class SchemaParserService : ISchemaParserService
{
    private readonly ILogger<SchemaParserService> _logger;
    private readonly HierarchicalCsvParser _hierarchicalCsvParser;

    public SchemaParserService(
        ILogger<SchemaParserService> logger,
        HierarchicalCsvParser hierarchicalCsvParser)
    {
        _logger = logger;
        _hierarchicalCsvParser = hierarchicalCsvParser;
    }

    public async Task<SchemaDefinition> ParseSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parsing schema: {Name} with type: {SourceType}", request.Name, request.SourceType);

        var schemaDefinition = request.SourceType.ToLowerInvariant() switch
        {
            "csv" => await ParseCsvSchemaAsync(request, cancellationToken),
            "xml" => await ParseXmlSchemaAsync(request, cancellationToken),
            "json" => await ParseJsonSchemaAsync(request, cancellationToken),
            "xsd" => await ParseXsdSchemaAsync(request, cancellationToken),
            _ => throw new NotSupportedException($"Source type '{request.SourceType}' is not supported")
        };

        _logger.LogInformation("Successfully parsed schema: {Name} with {FieldCount} fields", 
            request.Name, schemaDefinition.Fields.Count);

        return schemaDefinition;
    }

    private Task<SchemaDefinition> ParseCsvSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parsing CSV schema with hierarchical detection for: {Name}", request.Name);

        // Try hierarchical parsing first (for legacy specification CSVs)
        try
        {
            var hierarchicalNodes = _hierarchicalCsvParser.ParseHierarchicalCsv(request.Content);
            
            if (hierarchicalNodes.Count > 0)
            {
                _logger.LogInformation("Successfully parsed hierarchical CSV with {NodeCount} root nodes", hierarchicalNodes.Count);
                
                // Convert hierarchical structure to flat fields for compatibility
                var allFields = new List<SchemaField>();
                foreach (var node in hierarchicalNodes)
                {
                    allFields.AddRange(node.ToSchemaFields());
                }

                var schema = new SchemaDefinition
                {
                    Name = request.Name,
                    SourceType = request.SourceType,
                    Fields = allFields,
                    Metadata = new Dictionary<string, object>
                    {
                        ["originalFormat"] = "csv",
                        ["parsingStrategy"] = "hierarchical",
                        ["fieldCount"] = allFields.Count,
                        ["rootNodeCount"] = hierarchicalNodes.Count,
                        ["hierarchicalStructure"] = hierarchicalNodes,
                        ["parsedAt"] = DateTime.UtcNow
                    }
                };

                return Task.FromResult(schema);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Hierarchical parsing failed, falling back to simple CSV parsing");
        }

        // Fallback to simple CSV parsing (header-based)
        return ParseSimpleCsvSchemaAsync(request, cancellationToken);
    }

    private Task<SchemaDefinition> ParseSimpleCsvSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken)
    {
        // Parse CSV header line to extract field names
        var lines = request.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var headerLine = lines.FirstOrDefault() ?? string.Empty;
        var fieldNames = headerLine.Split(',', StringSplitOptions.TrimEntries);

        var fields = fieldNames.Select(name => new SchemaField
        {
            Name = name,
            DataType = "string", // CSV fields are typically strings
            IsRequired = false,
            IsArray = false
        }).ToList();

        var schema = new SchemaDefinition
        {
            Name = request.Name,
            SourceType = request.SourceType,
            Fields = fields,
            Metadata = new Dictionary<string, object>
            {
                ["originalFormat"] = "csv",
                ["parsingStrategy"] = "simple",
                ["fieldCount"] = fields.Count,
                ["parsedAt"] = DateTime.UtcNow
            }
        };

        return Task.FromResult(schema);
    }

    private Task<SchemaDefinition> ParseXmlSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken)
    {
        // Basic XML parsing - extract root element and child elements
        _logger.LogInformation("Parsing XML schema for: {Name}", request.Name);

        // Simplified XML parsing for demonstration
        var fields = new List<SchemaField>
        {
            new SchemaField
            {
                Name = "root",
                DataType = "object",
                IsRequired = true,
                IsArray = false,
                Description = "Root XML element"
            }
        };

        var schema = new SchemaDefinition
        {
            Name = request.Name,
            SourceType = request.SourceType,
            Fields = fields,
            Metadata = new Dictionary<string, object>
            {
                ["originalFormat"] = "xml",
                ["parsedAt"] = DateTime.UtcNow
            }
        };

        return Task.FromResult(schema);
    }

    private async Task<SchemaDefinition> ParseJsonSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parsing JSON schema for: {Name}", request.Name);

        try
        {
            // Parse JSON to extract structure
            using var document = JsonDocument.Parse(request.Content);
            var fields = ExtractFieldsFromJsonElement(document.RootElement);

            var schema = new SchemaDefinition
            {
                Name = request.Name,
                SourceType = request.SourceType,
                Fields = fields,
                Metadata = new Dictionary<string, object>
                {
                    ["originalFormat"] = "json",
                    ["fieldCount"] = fields.Count,
                    ["parsedAt"] = DateTime.UtcNow
                }
            };

            return await Task.FromResult(schema);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON content");
            throw new InvalidOperationException("Invalid JSON content", ex);
        }
    }

    private Task<SchemaDefinition> ParseXsdSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parsing XSD schema for: {Name}", request.Name);

        // Simplified XSD parsing for demonstration
        var fields = new List<SchemaField>
        {
            new SchemaField
            {
                Name = "schemaRoot",
                DataType = "complexType",
                IsRequired = true,
                IsArray = false,
                Description = "XSD schema root"
            }
        };

        var schema = new SchemaDefinition
        {
            Name = request.Name,
            SourceType = request.SourceType,
            Fields = fields,
            Metadata = new Dictionary<string, object>
            {
                ["originalFormat"] = "xsd",
                ["parsedAt"] = DateTime.UtcNow
            }
        };

        return Task.FromResult(schema);
    }

    private List<SchemaField> ExtractFieldsFromJsonElement(JsonElement element)
    {
        var fields = new List<SchemaField>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var field = new SchemaField
                {
                    Name = property.Name,
                    DataType = MapJsonTypeToDataType(property.Value.ValueKind),
                    IsRequired = false,
                    IsArray = property.Value.ValueKind == JsonValueKind.Array
                };

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    field.NestedFields = ExtractFieldsFromJsonElement(property.Value);
                }

                fields.Add(field);
            }
        }

        return fields;
    }

    private string MapJsonTypeToDataType(JsonValueKind kind)
    {
        return kind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Array => "array",
            JsonValueKind.Object => "object",
            JsonValueKind.Null => "null",
            _ => "unknown"
        };
    }
}
