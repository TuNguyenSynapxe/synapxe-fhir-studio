using System.Text.RegularExpressions;
using SampleGeneratorService.Models;

namespace SampleGeneratorService.Services;

public class SampleGeneratorService : ISampleGeneratorService
{
    private readonly ILogger<SampleGeneratorService> _logger;
    private readonly IOpenAiSampleGenerator _aiGenerator;
    private Random _random;

    public SampleGeneratorService(
        ILogger<SampleGeneratorService> logger,
        IOpenAiSampleGenerator aiGenerator)
    {
        _logger = logger;
        _aiGenerator = aiGenerator;
        _random = new Random(42); // Default seed for deterministic generation
    }

    public async Task<List<Dictionary<string, object>>> GenerateSamplesAsync(SampleGenerationRequest request)
    {
        // Reset random with seed for deterministic generation
        if (request.Seed.HasValue)
        {
            _random = new Random(request.Seed.Value);
        }

        var samples = new List<Dictionary<string, object>>();

        for (int i = 0; i < request.RecordCount; i++)
        {
            Dictionary<string, object> sample;

            if (request.HierarchicalSchema != null && request.HierarchicalSchema.Count > 0)
            {
                // Generate from hierarchical schema
                sample = GenerateFromHierarchicalSchema(request.HierarchicalSchema, request.UseAi);
            }
            else if (request.SchemaDefinition != null)
            {
                // Generate from flat schema definition
                sample = GenerateFromSchemaDefinition(request.SchemaDefinition, request.UseAi);
            }
            else
            {
                throw new ArgumentException("Either SchemaDefinition or HierarchicalSchema must be provided");
            }

            samples.Add(sample);
        }

        return await Task.FromResult(samples);
    }

    private Dictionary<string, object> GenerateFromHierarchicalSchema(List<SchemaNode> nodes, bool useAi)
    {
        var result = new Dictionary<string, object>();

        foreach (var node in nodes)
        {
            // If it's a single grouping node at root (like "patient" or "putEvent" wrapper), flatten it
            if (nodes.Count == 1 && IsGroupingNode(node) && node.Children.Count > 0 && !ParseCardinality(node.Cardinality).isArray)
            {
                // Process children directly to flatten the wrapper
                foreach (var child in node.Children)
                {
                    ProcessNode(child, result, useAi);
                }
            }
            else
            {
                // Otherwise process the node normally (preserving arrays, etc.)
                ProcessNode(node, result, useAi);
            }
        }

        return result;
    }

    private void ProcessNode(SchemaNode node, Dictionary<string, object> parent, bool useAi)
    {
        var (isRequired, isArray) = ParseCardinality(node.Cardinality);

        // Decide if we should include this field (required = 100%, optional = 50%)
        if (!isRequired && _random.NextDouble() > 0.5)
        {
            return;
        }

        // If node is a grouping/container with children, create nested object
        if (node.Children.Count > 0 && !IsLeafNode(node))
        {
            if (isArray)
            {
                // Generate 1-3 array elements
                var arrayCount = _random.Next(1, 4);
                var arrayItems = new List<Dictionary<string, object>>();

                for (int i = 0; i < arrayCount; i++)
                {
                    var childObject = new Dictionary<string, object>();
                    foreach (var child in node.Children)
                    {
                        ProcessNode(child, childObject, useAi);
                    }
                    if (childObject.Count > 0)
                    {
                        arrayItems.Add(childObject);
                    }
                }

                if (arrayItems.Count > 0)
                {
                    parent[node.Name] = arrayItems;
                }
            }
            else
            {
                // Nested object
                var childObject = new Dictionary<string, object>();
                foreach (var child in node.Children)
                {
                    ProcessNode(child, childObject, useAi);
                }
                if (childObject.Count > 0)
                {
                    parent[node.Name] = childObject;
                }
            }
        }
        else
        {
            // Leaf node - generate value
            var value = GenerateValue(node, useAi);

            if (isArray)
            {
                // Generate 1-3 array elements
                var arrayCount = _random.Next(1, 4);
                var arrayItems = new List<object>();
                for (int i = 0; i < arrayCount; i++)
                {
                    arrayItems.Add(GenerateValue(node, useAi));
                }
                parent[node.Name] = arrayItems;
            }
            else
            {
                parent[node.Name] = value;
            }
        }
    }

    private Dictionary<string, object> GenerateFromSchemaDefinition(SchemaDefinition schema, bool useAi)
    {
        var result = new Dictionary<string, object>();

        foreach (var field in schema.Fields)
        {
            ProcessField(field, result, useAi);
        }

        return result;
    }

    private void ProcessField(SchemaField field, Dictionary<string, object> parent, bool useAi)
    {
        // Decide if we should include this field (required = 100%, optional = 50%)
        if (!field.IsRequired && _random.NextDouble() > 0.5)
        {
            return;
        }

        if (field.NestedFields != null && field.NestedFields.Count > 0)
        {
            // Nested object
            var nestedObject = new Dictionary<string, object>();
            foreach (var nestedField in field.NestedFields)
            {
                ProcessField(nestedField, nestedObject, useAi);
            }
            parent[field.Name] = nestedObject;
        }
        else
        {
            var value = GenerateValueFromField(field, useAi);

            if (field.IsArray)
            {
                // Generate 1-3 array elements
                var arrayCount = _random.Next(1, 4);
                var arrayItems = new List<object>();
                for (int i = 0; i < arrayCount; i++)
                {
                    arrayItems.Add(GenerateValueFromField(field, useAi));
                }
                parent[field.Name] = arrayItems;
            }
            else
            {
                parent[field.Name] = value;
            }
        }
    }

    private object GenerateValue(SchemaNode node, bool useAi)
    {
        // If SampleValue exists, use it
        if (!string.IsNullOrWhiteSpace(node.SampleValue))
        {
            return CleanValue(node.SampleValue);
        }

        // Use AI generation if enabled
        if (useAi)
        {
            var aiValue = _aiGenerator.GenerateValue(node.Name, node.DataType, node.Definition);
            if (aiValue != null)
            {
                return CleanValue(aiValue);
            }
        }

        // Deterministic generation based on DataType
        return GenerateByDataType(node.DataType, node.Name);
    }

    private object GenerateValueFromField(SchemaField field, bool useAi)
    {
        // Use AI generation if enabled
        if (useAi)
        {
            var aiValue = _aiGenerator.GenerateValue(field.Name, field.DataType, field.Description);
            if (aiValue != null)
            {
                return CleanValue(aiValue);
            }
        }

        // Deterministic generation based on DataType
        return GenerateByDataType(field.DataType, field.Name);
    }

    private object GenerateByDataType(string? dataType, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return $"sample_{fieldName}";
        }

        var normalizedType = dataType.ToLowerInvariant().Trim();

        // Extract base type from formats like "String (50)" or "Long (15)"
        var baseType = Regex.Replace(normalizedType, @"\s*\(\d+\)", "").Trim();

        return baseType switch
        {
            "string" or "text" => $"sample_{fieldName}",
            "integer" or "int" or "long" or "numeric" => _random.Next(1, 1000),
            "decimal" or "double" or "float" or "number" => Math.Round(_random.NextDouble() * 1000, 2),
            "boolean" or "bool" => _random.Next(0, 2) == 1,
            "date" => DateTime.UtcNow.AddDays(-_random.Next(0, 365)).ToString("yyyy-MM-dd"),
            "datetime" => DateTime.UtcNow.AddDays(-_random.Next(0, 365)).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            "time" => DateTime.UtcNow.ToString("HH:mm:ss"),
            "guid" or "uuid" => Guid.NewGuid().ToString(),
            _ => $"sample_{fieldName}"
        };
    }

    private string CleanValue(string value)
    {
        // Trim and clean the value
        value = value.Trim();
        
        // Remove common wrapper characters from sample values
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            value = value[1..^1];
        }

        return value;
    }

    private bool IsGroupingNode(SchemaNode node)
    {
        return node.DataType?.Equals("Grouping", StringComparison.OrdinalIgnoreCase) == true;
    }

    private bool IsLeafNode(SchemaNode node)
    {
        // A node is a leaf if it has no children or if it's explicitly typed (not a grouping)
        return node.Children.Count == 0 || 
               (!string.IsNullOrWhiteSpace(node.DataType) && 
                !IsGroupingNode(node));
    }

    private (bool isRequired, bool isArray) ParseCardinality(string? cardinality)
    {
        if (string.IsNullOrWhiteSpace(cardinality))
        {
            return (false, false);
        }

        bool isArray = cardinality.Contains("…") || 
                       cardinality.Contains("*") || 
                       cardinality.Contains("..") ||
                       Regex.IsMatch(cardinality, @"\d+\s*…\s*\d+") ||
                       Regex.IsMatch(cardinality, @"\d+\s*\.\.\s*\d+");

        bool isRequired = cardinality.StartsWith("1") || 
                         cardinality.Contains("Mandatory", StringComparison.OrdinalIgnoreCase) ||
                         cardinality.Contains("Required", StringComparison.OrdinalIgnoreCase);

        return (isRequired, isArray);
    }
}
