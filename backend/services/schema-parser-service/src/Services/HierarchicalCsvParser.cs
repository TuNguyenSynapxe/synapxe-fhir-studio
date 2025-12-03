using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace SchemaParserService.Services;

/// <summary>
/// Parses hierarchical CSV files with column-based indentation (legacy specification format)
/// </summary>
public class HierarchicalCsvParser
{
    private readonly ILogger<HierarchicalCsvParser> _logger;

    public HierarchicalCsvParser(ILogger<HierarchicalCsvParser> logger)
    {
        _logger = logger;
    }

    public List<Models.SchemaNode> ParseHierarchicalCsv(string csvContent)
    {
        var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return new List<Models.SchemaNode>();
        }

        // Parse CSV into raw records
        var records = ParseCsvLines(lines);
        if (records.Count == 0)
        {
            return new List<Models.SchemaNode>();
        }

        // Find the first non-empty row for column detection (skip empty leading rows)
        var headerRecord = records.FirstOrDefault(r => !IsEmptyRow(r));
        if (headerRecord == null)
        {
            _logger.LogWarning("No non-empty rows found in CSV");
            return new List<Models.SchemaNode>();
        }

        // Find column indices for metadata
        var columnMapping = DetectColumnMapping(headerRecord);
        
        // Build hierarchy
        var rootNodes = BuildHierarchy(records, columnMapping);

        return rootNodes;
    }

    /// <summary>
    /// Trims all whitespace including hidden characters
    /// </summary>
    private string? TrimAll(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Remove all types of whitespace including \r, \n, \t, and Unicode whitespace
        value = value.Trim();
        value = Regex.Replace(value, @"\s+", " "); // Normalize internal whitespace
        
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Normalizes cardinality values to handle various formats
    /// </summary>
    private string? NormalizeCardinality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = TrimAll(value);
        if (value == null)
            return null;

        // Handle various formats: "1", "1..1", "1 … 1", "Mandatory", "Optional", "Required", etc.
        value = value.Replace("…", "...").Replace("..", "...").Trim();

        return value;
    }

    /// <summary>
    /// Parses data type to extract base type and handle various formats
    /// </summary>
    private string? IntelligentDataTypeParser(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = TrimAll(value);
        if (value == null)
            return null;

        // Handle trailing spaces and normalize
        value = value.Trim();

        return value;
    }

    /// <summary>
    /// Checks if a row is effectively empty (no meaningful content)
    /// </summary>
    private bool IsEmptyRow(Dictionary<int, string> record)
    {
        // A row is empty if all columns are null, empty, or whitespace
        return record.Values.All(v => string.IsNullOrWhiteSpace(v));
    }

    /// <summary>
    /// Checks if a row represents a grouping container
    /// </summary>
    private bool IsGroupingRow(string? dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
            return false;

        var normalized = TrimAll(dataType);
        return normalized?.Equals("Grouping", StringComparison.OrdinalIgnoreCase) == true;
    }

    private List<Dictionary<int, string>> ParseCsvLines(string[] lines)
    {
        var records = new List<Dictionary<int, string>>();

        using var reader = new StringReader(string.Join("\n", lines));
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            MissingFieldFound = null,
            BadDataFound = null
        });

        while (csv.Read())
        {
            var record = new Dictionary<int, string>();
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                var value = csv.GetField(i)?.Trim() ?? string.Empty;
                record[i] = value;
            }
            records.Add(record);
        }

        return records;
    }

    private ColumnMapping DetectColumnMapping(Dictionary<int, string> headerRow)
    {
        var mapping = new ColumnMapping();

        // Search for known column headers
        foreach (var kvp in headerRow)
        {
            var value = TrimAll(kvp.Value)?.ToLowerInvariant().Replace(" ", "").Replace("\n", "").Replace("\r", "") ?? "";
            
            if (value.Contains("elementname"))
                mapping.ElementNameStartCol = kvp.Key;
            else if (value.Contains("definition") && !value.Contains("fhir"))
                mapping.DefinitionsCol = kvp.Key;
            else if (value.Contains("remarks") || value.Contains("samplevalue"))
                mapping.RemarksCol = kvp.Key;
            else if (value.Contains("significance"))
                mapping.SignificanceCol = kvp.Key;
            else if (value.Contains("cardinality"))
                mapping.CardinalityCol = kvp.Key;
            else if (value.Contains("datatype") || value.Contains("data type"))
                mapping.DataTypeCol = kvp.Key;
            else if (value.Contains("fhirmapping") || value.Contains("fhir mapping"))
                mapping.FhirMappingCol = kvp.Key;
        }

        _logger.LogDebug("Column mapping detected: ElementName={ElementName}, Definitions={Definitions}, DataType={DataType}, FHIR={FHIR}",
            mapping.ElementNameStartCol, mapping.DefinitionsCol, mapping.DataTypeCol, mapping.FhirMappingCol);

        return mapping;
    }

    private List<Models.SchemaNode> BuildHierarchy(List<Dictionary<int, string>> records, ColumnMapping columnMapping)
    {
        var rootNodes = new List<Models.SchemaNode>();
        var nodeStack = new Stack<(Models.SchemaNode Node, int Level)>();
        var headerSkipped = false;

        foreach (var record in records)
        {
            // Skip empty rows
            if (IsEmptyRow(record))
                continue;

            // Skip header row - detect by checking if first column contains "Element Name"
            if (!headerSkipped)
            {
                var firstValue = GetColumnValue(record, 0);
                if (firstValue?.Contains("Element", StringComparison.OrdinalIgnoreCase) == true ||
                    firstValue?.Contains("Name", StringComparison.OrdinalIgnoreCase) == true)
                {
                    headerSkipped = true;
                    continue;
                }
                // If it's not a header row, process it as data and mark header as skipped
                headerSkipped = true;
            }

            // Detect element name and level based on first non-empty column
            var (elementName, level) = DetectElementAndLevel(record, columnMapping.ElementNameStartCol);
            
            if (string.IsNullOrWhiteSpace(elementName))
                continue;

            // Skip header repetitions
            if (elementName.Equals("Element Name", StringComparison.OrdinalIgnoreCase))
                continue;

            // Extract and normalize metadata
            var dataType = IntelligentDataTypeParser(GetColumnValue(record, columnMapping.DataTypeCol));
            var cardinality = NormalizeCardinality(GetColumnValue(record, columnMapping.CardinalityCol));
            var definition = TrimAll(GetColumnValue(record, columnMapping.DefinitionsCol));
            var remarks = TrimAll(GetColumnValue(record, columnMapping.RemarksCol));
            var significance = TrimAll(GetColumnValue(record, columnMapping.SignificanceCol));
            var fhirMapping = TrimAll(GetColumnValue(record, columnMapping.FhirMappingCol));

            // Check if this is a grouping row
            if (IsGroupingRow(dataType))
            {
                // Create a grouping node but mark it as a container
                var groupingNode = new Models.SchemaNode
                {
                    Name = elementName,
                    Level = level,
                    DataType = dataType,
                    Cardinality = cardinality,
                    Definition = definition,
                    SampleValue = ExtractSampleValue(remarks),
                    Significance = significance,
                    FhirMapping = fhirMapping
                };

                // Pop nodes until we find the correct parent
                while (nodeStack.Count > 0 && nodeStack.Peek().Level >= level)
                {
                    nodeStack.Pop();
                }

                if (nodeStack.Count == 0)
                {
                    rootNodes.Add(groupingNode);
                }
                else
                {
                    nodeStack.Peek().Node.Children.Add(groupingNode);
                }

                nodeStack.Push((groupingNode, level));
                continue;
            }

            // Create node for actual fields
            var node = new Models.SchemaNode
            {
                Name = elementName,
                Level = level,
                DataType = dataType,
                Cardinality = cardinality,
                Definition = definition,
                SampleValue = ExtractSampleValue(remarks),
                Significance = significance,
                FhirMapping = fhirMapping
            };

            // Pop nodes until we find the correct parent level
            while (nodeStack.Count > 0 && nodeStack.Peek().Level >= level)
            {
                nodeStack.Pop();
            }

            // Add to parent or root
            if (nodeStack.Count == 0)
            {
                rootNodes.Add(node);
            }
            else
            {
                nodeStack.Peek().Node.Children.Add(node);
            }

            // Push current node to stack for potential children
            nodeStack.Push((node, level));
        }

        return rootNodes;
    }

    private (string ElementName, int Level) DetectElementAndLevel(Dictionary<int, string> record, int startCol)
    {
        // Find first non-empty column starting from the element name section
        for (int col = startCol; col < startCol + 20; col++) // Check up to 20 columns for hierarchy
        {
            if (record.TryGetValue(col, out var value))
            {
                var trimmed = TrimAll(value);
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    // Level is determined by column offset from start
                    int level = col - startCol;
                    return (trimmed, level);
                }
            }
        }

        return (string.Empty, 0);
    }

    private string? GetColumnValue(Dictionary<int, string> record, int? columnIndex)
    {
        if (!columnIndex.HasValue || !record.TryGetValue(columnIndex.Value, out var value))
            return null;

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? ExtractSampleValue(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
            return null;

        remarks = TrimAll(remarks);
        if (remarks == null)
            return null;

        // Look for "Sample Value:" pattern
        var sampleValueMatch = Regex.Match(
            remarks, 
            @"Sample\s+Value[:\s]+[""']?([^""'\r\n]+)[""']?",
            RegexOptions.IgnoreCase);

        if (sampleValueMatch.Success)
        {
            return TrimAll(sampleValueMatch.Groups[1].Value);
        }

        return null;
    }

    private class ColumnMapping
    {
        public int ElementNameStartCol { get; set; } = 0;
        public int? DefinitionsCol { get; set; }
        public int? RemarksCol { get; set; }
        public int? SignificanceCol { get; set; }
        public int? CardinalityCol { get; set; }
        public int? DataTypeCol { get; set; }
        public int? FhirMappingCol { get; set; }
    }
}
