namespace MappingService.Models;

public class MappingItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourcePath { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public string? TransformationExpression { get; set; }
    public string? Notes { get; set; }
    public bool IsRequired { get; set; }
}
