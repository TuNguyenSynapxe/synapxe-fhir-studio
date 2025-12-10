using Newtonsoft.Json.Linq;

namespace MappingEngineService.DTOs;

public class MappingExecutionRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string MappingId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public JObject SourcePayload { get; set; } = new();
    public string? SchemaId { get; set; }
}
