namespace SchemaParserService.Models;

public class ParseSchemaRequest
{
    public string SourceType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
