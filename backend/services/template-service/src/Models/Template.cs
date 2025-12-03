using Newtonsoft.Json.Linq;

namespace TemplateService.Models;

public class Template
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = "R4"; // R4 only in Phase 1
    public JObject TemplateContent { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
